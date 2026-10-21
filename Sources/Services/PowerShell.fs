namespace Belin.Cli.Services

open Belin.Cli
open System
open System.IO
open System.Management.Automation

/// Represents a PowerShell service.
type PowerShellService(path: string) as this =
  inherit Service(path)

  /// The path of the service entry point.
  let mutable entryPath = ""

  do
    let manifestPath =
      Directory.EnumerateFiles(this.Path, "*.psd1")
      |> Seq.filter (fun file ->
        let name = Path.GetFileNameWithoutExtension file
        name <> "PSModules" && name <> "PSScriptAnalyzerSettings")
      |> Seq.tryHead

    match manifestPath with
    | None -> ()
    | Some filePath ->
      let manifest = Automation.parsePowerShellDataFile filePath
      let rootModule = string manifest["RootModule"]
      if String.IsNullOrWhiteSpace this.Manifest.Description then this.Manifest.Description <- string manifest["Description"]
      if String.IsNullOrWhiteSpace this.Manifest.Name then this.Manifest.Name <- Path.GetFileNameWithoutExtension filePath |> defaultIfNull ""
      if not (String.IsNullOrWhiteSpace rootModule) then entryPath <- Path.GetFullPath(rootModule, this.Path)

  /// The entry point of this service.
  override val EntryPoint =
    if entryPath.Length > 0 then $@"-ExecutionPolicy Bypass -File ""{entryPath}"" -NoLogo -NoProfile -NonInteractive"
    else raise (EntryPointNotFoundException "Unable to resolve the service entry point.")

  /// The name of the environment variable storing the service environment.
  override val EnvironmentVariable = "PODE_ENVIRONMENT"

  /// The program used to run this service.
  override val Program = if Platform.IsWindows then "pwsh.exe" else "pwsh"
