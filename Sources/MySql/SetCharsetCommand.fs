namespace Belin.Cli.MySql

open Belin.Sql
open MySqlConnector
open System
open System.CommandLine

/// Alters the character set of MariaDB/MySQL tables.
type SetCharsetCommand() as this =
  inherit Command("set-charset", "Alter the character set of MariaDB/MySQL tables.")

  /// The name of the new character set.
  let collationArgument = Argument<string>("collation", Description = "The name of the new character set.")

  /// The connection URI.
  let uriOption = UriOption()

  /// The schema name.
  let schemaOption = SchemaOption()

  /// The table name.
  let tableOption = TableOption()

  do
    this.Arguments.Add collationArgument
    this.Options.Add uriOption
    this.Options.Add schemaOption
    this.Options.Add tableOption
    this.SetAction this.Invoke

  /// Invokes this command.
  member _.Invoke(parseResult: ParseResult): int =
    use connection = MySqlConnection.OfUri (Uri(parseResult.GetRequiredValue uriOption))

    let collation = parseResult.GetRequiredValue collationArgument
    let normalizedCollation = collation.ToLowerInvariant()
    if not (connection.GetCollations() |> Seq.exists (fun collation -> collation.ToLowerInvariant() = normalizedCollation)) then
      Console.Error.WriteLine "The specified collation is unknown."
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
          ALTER TABLE {table.GetQualifiedName(escape = true)} CONVERT TO CHARACTER SET {Array.head (collation.Split '_')} COLLATE {collation};
          SET foreign_key_checks = 1;"

        Console.WriteLine $"Processing: {table.QualifiedName}"
        connection.Execute sql |> ignore<int>
      0
