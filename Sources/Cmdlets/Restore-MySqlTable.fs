namespace Belin.Cli.Cmdlets

open Belin.Cli
open System
open System.Management.Automation

/// Restores a set of MariaDB/MySQL tables.
[<Cmdlet(VerbsData.Restore, "MySqlTable"); OutputType(typeof<Void>)>]
type RestoreMySqlTable() =
  inherit Cmdlet()

  /// The connection URI.
  [<Parameter(Mandatory = true, Position = 1); ValidateConnectionUri("The specified connection URI is invalid.")>]
  member val Uri: Uri | null = null with get, set

  /// Specifies the path to an SQL dump.
  [<Parameter(Mandatory = true, ParameterSetName = "Path", Position = 2, ValueFromPipeline = true); SupportsWildcards>]
  member val Path: string array = [||] with get, set

  /// Specifies the literal path to an SQL dump.
  [<Parameter(Mandatory = true, ParameterSetName = "LiteralPath"); ValidatePath("The specified literal path is invalid.")>]
  member val LiteralPath: string array = [||] with get, set

  /// A pattern used to filter the list of files to be processed.
  [<Parameter>]
  member val Filter = "*.sql" with get, set

  /// Value indicating whether to process the input path recursively.
  [<Parameter>]
  member val Recurse = SwitchParameter false with get, set
