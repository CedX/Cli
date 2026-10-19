namespace Belin.Cli.Cmdlets

open Belin.Cli
open System
open System.Management.Automation

/// TODO
[<Cmdlet(VerbsCommon.Set, "MySqlCharset"); OutputType(typeof<Void>)>]
type SetMySqlCharset() =
  inherit Cmdlet()
