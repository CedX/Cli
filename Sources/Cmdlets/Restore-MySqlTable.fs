namespace Belin.Cli.Cmdlets

open Belin.Cli
open System
open System.Management.Automation

/// TODO
[<Cmdlet(VerbsData.Restore, "MySqlTable"); OutputType(typeof<Void>)>]
type RestoreMySqlTable() =
  inherit Cmdlet()
