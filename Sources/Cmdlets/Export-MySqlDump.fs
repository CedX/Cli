namespace Belin.Cli.Cmdlets

open Belin.Cli
open System
open System.Management.Automation

/// TODO
[<Cmdlet(VerbsData.Export, "MySqlDump"); OutputType(typeof<Void>)>]
type ExportMySqlDump() =
  inherit Cmdlet()
