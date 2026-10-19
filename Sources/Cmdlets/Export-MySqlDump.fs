namespace Belin.Cli.Cmdlets

open Belin.Cli
open System
open System.Management.Automation

/// Exports the specified schema to a SQL dump in the specified directory.
[<Cmdlet(VerbsData.Export, "MySqlDump"); OutputType(typeof<Void>)>]
type ExportMySqlDump() =
  inherit Cmdlet()

  /// The database schema.
  [<Parameter(Mandatory = true, Position = 1)>]
  member val Schema: MySqlSchema | null = null with get, set

  /// The path to the output directory.
  [<Parameter(Mandatory = true, Position = 2); ValidatePath("The specified output path is invalid.")>]
  member val Path = "" with get, set

  /// The connection URI.
  [<Parameter(Mandatory = true); ValidateConnectionUri("The specified connection URI is invalid.")>]
  member val Uri: Uri | null = null with get, set

  /// The table name.
  [<Parameter>]
  member val Table: string array = [||] with get, set
