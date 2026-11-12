namespace Belin.Cli.MySql

open Belin.Cli
open Belin.Cli.MySql
open Belin.Sql
open MySqlConnector
open System
open System.CommandLine
open System.Data
open System.Management.Automation

/// Alters the storage engine of MariaDB/MySQL tables.
type MySqlSetEngineCommand() =
  inherit Command("set-engine", "Alter the storage engine of MariaDB/MySQL tables.")

  /// The connection URI.
  let uriOption = UriOption()

  /// The database connection.
  let mutable dbConnection: IDbConnection | null = null

  /// The name of the new storage engine.
  [<Parameter(Mandatory = true, Position = 2)>]
  member val Engine = "" with get, set

  /// The schema name.
  let schemaOption = SchemaOption()

  /// The table name.
  let tableOption = TableOption()

  interface IDisposable with
    /// Releases any resources associated with this object.
    member _.Dispose() = match dbConnection with null -> () | disposable -> disposable.Dispose()

  /// Performs initialization of command execution.
  override this.BeginProcessing() =
    let connection = MySqlConnection.OfUri(nonNull this.Uri)
    dbConnection <- connection

    let normalizedEngine = this.Engine.ToLowerInvariant()
    if not (connection.GetEngines() |> Seq.exists (fun engine -> engine.ToLowerInvariant() = normalizedEngine)) then
      let ex = ArgumentOutOfRangeException (nameof this.Engine)
      ErrorRecord(ex, "ArgumentOutOfRange", ErrorCategory.InvalidArgument, this.Engine)

  /// Invokes this command.
  member _.Invoke(parseResult: ParseResult): int =
    let connection = nonNull dbConnection
    let schemas =
      if Array.isEmpty this.Schema then InformationSchema.getSchemas connection
      else this.Schema |> Seq.map (fun schema -> MySqlSchema(Name = schema))

    let tables =
      schemas |> Seq.collect (fun schema ->
        if Array.isEmpty this.Table then InformationSchema.getTables(connection, schema)
        else this.Table |> Seq.map (fun table -> MySqlTable(Name = table, Schema = schema.Name)))

    for table in tables do
      let sql = $"
        SET foreign_key_checks = 0;
        ALTER TABLE {table.GetQualifiedName(escape = true)} ENGINE = {this.Engine};
        SET foreign_key_checks = 1;"

      Console.WriteLine $"Processing: {table.QualifiedName}"
      connection.Execute sql |> ignore<int>
