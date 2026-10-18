namespace Belin.Cli.Cmdlets

open Belin.Cli
open System
open System.Management.Automation
open System.Net.Http

/// Represents a Node.js release.
type NodeRelease = {

  /// The version number.
  Version: string
}

/// Downloads and installs the latest Node.js release.
/// Returns the output from the `node --version` command.
[<Cmdlet(VerbsLifecycle.Install, "Node"); OutputType(typeof<Void>)>]
type InstallNode() =
  inherit Cmdlet()

  /// The HTTP client used to download the archive.
  let mutable httpClient: HttpClient | null = null

  /// The path to the output directory.
  [<Parameter(Position = 1); ValidatePath("The specified output path is invalid.")>]
  member val DestinationPath = if OperatingSystem.IsWindows() then @"C:\Program Files\Node.js" else "/usr/local"
    with get, set

  /// Performs initialization of command execution.
  override _.BeginProcessing() = httpClient <- Http.newClient()

  /// Performs clean-up after the command execution.
  override _.EndProcessing() = match httpClient with null -> () | value -> value.Dispose()

  /// Performs execution of this command.
  override this.ProcessRecord() =
    ()

  member private _.FetchLatestVersion() =
    ()
