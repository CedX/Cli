namespace Belin.Cli.Cmdlets

open Belin.Cli
open System
open System.Management.Automation

/// TODO
[<Cmdlet(VerbsCommon.Optimize, "MySqlTable"); OutputType(typeof<Void>)>]
type OptimizeMySqlTable() =
  inherit Cmdlet()
