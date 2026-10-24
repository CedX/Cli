namespace Belin.Cli

open System
open System.Collections
open System.Management.Automation.Language

/// Contains operations for working with the PowerShell environment.
module Automation =

  /// Parses the PowerShell data file located at the specified path.
  let parsePowerShellDataFile (path: string): Result<Hashtable, string> =
    let scriptBlockAst, _, errors = Parser.ParseFile path
    match Array.isEmpty errors with
    | false -> Error (errors |> Array.map _.Message |> String.concat Environment.NewLine)
    | true ->
      match scriptBlockAst.Find((fun ast -> ast :? HashtableAst), searchNestedScriptBlocks = false) with
      | :? HashtableAst as hashtableAst -> Ok (hashtableAst.SafeGetValue() :?> Hashtable)
      | _ -> Error $@"The file ""{path}"" could not be parsed as a hash table."
