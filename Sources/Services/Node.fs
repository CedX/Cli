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
  let mutable entryPath = ""

  do
    let packagePath = Path.Join(this.Path, "package.json")
    let packageManifest =
      match File.Exists packagePath with
      | false -> None
      | true ->
        let package = JsonSerializer.Deserialize<NodePackage>(File.ReadAllText packagePath, JsonSerializerOptions.Web)
        Some (package |> defaultIfNull { Bin = None; Description = ""; Name = "" })

    match packageManifest with
    | None -> ()
    | Some package ->
      if String.IsNullOrWhiteSpace this.Manifest.Description then this.Manifest.Description <- package.Description
      if String.IsNullOrWhiteSpace this.Manifest.Name then this.Manifest.Name <- package.Name
      match package.Bin with
      | None -> ()
      | Some dictionary when dictionary.Count = 0 -> ()
      | Some dictionary -> entryPath <- Path.GetFullPath(dictionary |> Seq.head |> _.Value, this.Path)

  /// The entry point of this service.
  override val EntryPoint =
    if entryPath.Length > 0 then entryPath
    else raise (EntryPointNotFoundException "Unable to resolve the service entry point.")

  /// The name of the environment variable storing the service environment.
  override val EnvironmentVariable = "NODE_ENV"

  /// The program used to run this service.
  override val Program = if Platform.IsWindows then "node.exe" else "node"
