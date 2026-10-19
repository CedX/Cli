namespace Belin.Cli.Cmdlets

open Belin.Cli
open Belin.Cli.MySql
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
  [<Parameter(Mandatory = true, Position = 1); ValidateConnectionUri("The specified connection URI is invalid.")>]
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
  override this.BeginProcessing() = dbConnection <- MySqlConnection.OfUri(nonNull this.Uri)
