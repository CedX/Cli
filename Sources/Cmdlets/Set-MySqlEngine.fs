namespace Belin.Cli.Cmdlets

open Belin.Cli
open Belin.Cli.MySql
open Belin.Sql
open MySqlConnector
open System
open System.Data
open System.Management.Automation

/// Alters the storage engine of MariaDB/MySQL tables.
[<Cmdlet(VerbsCommon.Set, "MySqlEngine"); OutputType(typeof<Void>); Sealed>]
type SetMySqlEngine() =
  inherit Cmdlet()

  /// The database connection.
  let mutable dbConnection: IDbConnection | null = null

  /// The connection URI.
  [<Parameter(Mandatory = true, Position = 1); ValidateConnectionUri>]
  member val Uri: Uri | null = null with get, set

  /// The name of the new storage engine.
  [<Parameter(Mandatory = true, Position = 2)>]
  member val Engine = "" with get, set

  /// The schema name.
  [<Parameter>]
  member val Schema: string array = [||] with get, set

  /// The table name.
  [<Parameter>]
  member val Table: string array = [||] with get, set

  interface IDisposable with
    /// Releases any resources associated with this object.
    member _.Dispose() = match dbConnection with null -> () | disposable -> disposable.Dispose()

  /// Performs initialization of command execution.
  override this.BeginProcessing() =
    let connection = MySqlConnection.OfUri(nonNull this.Uri)
    dbConnection <- connection

    let normalizedEngine = this.Engine.ToLowerInvariant()
    if not (connection.GetEngines() |> Seq.exists (fun engine -> engine.ToLowerInvariant() = normalizedEngine)) then
      raise (ArgumentOutOfRangeException (nameof this.Engine))

  /// Performs execution of this command.
  override this.ProcessRecord() =
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

      InformationRecord($"Processing: {table.QualifiedName}", nameof this.ProcessRecord) |> this.WriteInformation
      connection.Execute sql |> ignore<int>
