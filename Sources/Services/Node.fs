namespace Belin.Cli.Services

open Belin.Cli
open System
open System.Collections.Generic
open System.IO
open System.Management.Automation
open System.Text.Json

/// Represents the contents of a `package.json` file.
type NodePackage = {

  /// The dictionary of package commands.
  Bin: Dictionary<string, string> option

  /// The package description.
  Description: string

  /// The package name.
  Name: string
}

/// Represents a Node.js service.
type NodeService(path: string) as this =
  inherit Service(path)

  /// The path of the service entry point.
  let mutable entryPoint = ""

  do
    let manifest =
      Path.Join(this.Path, "package.json")
      |> File.ReadAllText
      |> fun json -> JsonSerializer.Deserialize<NodePackage>(json, JsonSerializerOptions.Web)
      |> defaultIfNull { Bin = None; Description = ""; Name = "" }

    if String.IsNullOrWhiteSpace this.Manifest.Description then this.Manifest.Description <- manifest.Description
    if String.IsNullOrWhiteSpace this.Manifest.Name then this.Manifest.Name <- manifest.Name
    match manifest.Bin with
    | None -> ()
    | Some dictionary when dictionary.Count = 0 -> ()
    | Some dictionary -> entryPoint <- dictionary |> Seq.head |> fun entry -> Path.GetFullPath(entry.Value, this.Path)

  /// The entry point of this service.
  override val EntryPoint =
    if entryPoint.Length > 0 then entryPoint
    else raise (EntryPointNotFoundException "Unable to resolve the service entry point.")

  /// The name of the environment variable storing the service environment.
  override val EnvironmentVariable = "NODE_ENV"

  /// The program used to run this service.
  override val Program = if Platform.IsWindows then "node.exe" else "node"
