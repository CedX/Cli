namespace Belin.Cli

open System
open System.Collections
open System.Management.Automation.Language

/// Contains operations for working with the PowerShell environment.
module Automation =

  /// Parses the PowerShell data file located at the specified path.
  let parsePowerShellDataFile (path: string): Hashtable =
    let scriptBlockAst, _, errors = Parser.ParseFile path
    if not (Array.isEmpty errors) then raise (FormatException (errors |> Array.map _.Message |> String.concat Environment.NewLine))
    match scriptBlockAst.Find((fun ast -> ast :? HashtableAst), searchNestedScriptBlocks = false) with
    | :? HashtableAst as hashtableAst -> hashtableAst.SafeGetValue() :?> Hashtable
    | _ -> raise (FormatException "The PowerShell data file is invalid.")
