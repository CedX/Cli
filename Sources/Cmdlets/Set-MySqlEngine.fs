namespace Belin.Cli.Cmdlets

open Belin.Cli
open System
open System.Management.Automation

/// TODO
[<Cmdlet(VerbsCommon.Set, "MySqlEngine"); OutputType(typeof<Void>)>]
type SetMySqlEngine() =
  inherit Cmdlet()
