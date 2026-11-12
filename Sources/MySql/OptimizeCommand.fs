namespace Belin.Cli.MySql

open Belin.Cli
open Belin.Sql
open MySqlConnectionExtensions
open MySqlConnector
open System
open System.CommandLine

/// Optimizes a set of MariaDB/MySQL tables.
type MySqlOptimizeCommand() as this =
  inherit Command("optimize", "Optimize a set of MariaDB/MySQL tables.")

  /// The connection URI.
  let uriOption = UriOption()

  /// The schema name.
  let schemaOption = SchemaOption()

  /// The table name.
  let tableOption = TableOption()

  do
    this.Options.Add uriOption
    this.Options.Add schemaOption
    this.Options.Add tableOption
    this.SetAction this.Invoke

  /// Invokes this command.
  member _.Invoke(parseResult: ParseResult): int =
    use connection = MySqlConnection.OfUri (Uri(parseResult.GetRequiredValue uriOption))

    let schemaNames = parseResult.GetRequiredValue schemaOption
    let schemas =
      if Array.isEmpty schemaNames then InformationSchema.getSchemas connection
      else schemaNames |> Seq.map (fun schema -> { MySqlSchema.Default with Name = schema })

    let tableNames = parseResult.GetRequiredValue tableOption
    let tables =
      schemas |> Seq.collect (fun schema ->
        if Array.isEmpty tableNames then InformationSchema.getTables(connection, schema)
        else tableNames |> Seq.map (fun table -> { MySqlTable.Default with Name = table; Schema = schema.Name}))

    for table in tables do
      Console.WriteLine $"Optimizing: {table.QualifiedName}"
      connection.Execute $"OPTIMIZE TABLE {table.GetQualifiedName(escape = true)}" |> ignore<int>
    0
