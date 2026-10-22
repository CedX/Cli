namespace Belin.Cli

open System
open System.Collections
open System.Management.Automation
open System.Management.Automation.Language

/// Contains operations for working with the PowerShell environment.
module Automation =

  /// Gets application information for the specified command.
  let getApplicationInfo (command: string): ApplicationInfo option =
    ScriptBlock.Create("Get-Command $_").Invoke [| command |]
    |> Seq.map (fun psObject -> psObject.BaseObject :?> ApplicationInfo)
    |> Seq.tryHead

  /// Parses the PowerShell data file located at the specified path.
  let parsePowerShellDataFile (path: string): Result<Hashtable, string> =
    let scriptBlockAst, _, errors = Parser.ParseFile path
    match Array.isEmpty errors with
    | false -> Error (errors |> Array.map _.Message |> String.concat Environment.NewLine)
    | true ->
      match scriptBlockAst.Find((fun ast -> ast :? HashtableAst), searchNestedScriptBlocks = false) with
      | :? HashtableAst as hashtableAst -> Ok (hashtableAst.SafeGetValue() :?> Hashtable)
      | _ -> Error "The PowerShell data file is invalid."
