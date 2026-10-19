namespace Belin.Cli.Cmdlets

open Belin.Cli
open System
open System.Management.Automation

/// TODO
[<Cmdlet(VerbsCommon.Remove, "NssmService"); OutputType(typeof<Void>)>]
type RemoveNssmService() =
  inherit Cmdlet()
