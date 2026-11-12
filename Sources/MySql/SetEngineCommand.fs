namespace Belin.Cli.MySql

open Belin.Sql
open MySqlConnector
open System
open System.CommandLine

/// Alters the storage engine of MariaDB/MySQL tables.
type SetEngineCommand() as this =
  inherit Command("set-engine", "Alter the storage engine of MariaDB/MySQL tables.")

  /// The name of the new storage engine.
  let engineArgument = Argument<string>("engine", Description = "The name of the new storage engine.")

  /// The connection URI.
  let uriOption = UriOption()

  /// The schema name.
  let schemaOption = SchemaOption()

  /// The table name.
  let tableOption = TableOption()

  do
    this.Arguments.Add engineArgument
    this.Options.Add uriOption
    this.Options.Add schemaOption
    this.Options.Add tableOption
    this.SetAction this.Invoke

  /// Invokes this command.
  member _.Invoke(parseResult: ParseResult): int =
    use connection = MySqlConnection.OfUri (Uri(parseResult.GetRequiredValue uriOption))

    let engine = parseResult.GetRequiredValue engineArgument
    let normalizedEngine = engine.ToLowerInvariant()
    if not (connection.GetEngines() |> Seq.exists (fun engine -> engine.Name.ToLowerInvariant() = normalizedEngine)) then
      eprintfn "The specified engine is unknown."
      1
    else
      let schemaNames = parseResult.GetRequiredValue schemaOption
      let schemas =
        if Array.isEmpty schemaNames then connection.GetSchemas()
        else schemaNames |> Seq.map (fun schema -> { Schema.Default with Name = schema })

      let tableNames = parseResult.GetRequiredValue tableOption
      let tables =
        schemas |> Seq.collect (fun schema ->
          if Array.isEmpty tableNames then connection.GetTables schema
          else tableNames |> Seq.map (fun table -> { Table.Default with Name = table; Schema = schema.Name}))

      for table in tables do
        let sql = $"
          SET foreign_key_checks = 0;
          ALTER TABLE {table.GetQualifiedName(escape = true)} ENGINE = {engine};
          SET foreign_key_checks = 1;"

        printn $"Processing: {table.QualifiedName}"
        connection.Execute sql |> ignore<int>
      0
