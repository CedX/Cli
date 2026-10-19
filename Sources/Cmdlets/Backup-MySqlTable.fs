namespace Belin.Cli.Cmdlets

open Belin.Cli
open System
open System.Management.Automation

/// TODO
[<Cmdlet(VerbsData.Backup, "MySqlTable"); OutputType(typeof<Void>)>]
type BackupMySqlTable() =
  inherit Cmdlet()
