namespace Belin.Cli.Cmdlets

open Belin.Cli
open System
open System.Management.Automation

/// TODO
[<Cmdlet(VerbsCommon.New, "NssmService"); OutputType(typeof<Void>)>]
type NewNssmService() =
  inherit Cmdlet()
