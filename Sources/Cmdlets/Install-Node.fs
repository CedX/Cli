namespace Belin.Cli.Cmdlets

open Belin.Cli
open System
open System.IO
open System.Management.Automation
open System.Net.Http
open System.Net.Http.Json
open System.Threading.Tasks

/// Represents a Node.js release.
type NodeRelease = {

  /// The version number.
  Version: string
}

/// Downloads and installs the latest Node.js release.
/// Returns the output from the `node --version` command.
[<Cmdlet(VerbsLifecycle.Install, "Node"); OutputType(typeof<string>)>]
type InstallNode() =
  inherit PSCmdlet()

  /// The HTTP client used to download the archive.
  let mutable httpClient: HttpClient | null = null

  /// The path to the output directory.
  [<Parameter(Position = 1); ValidatePath("The specified output path is invalid.")>]
  member val DestinationPath = if OperatingSystem.IsWindows() then @"C:\Program Files\Node.js" else "/usr/local" with get, set

  /// Performs initialization of command execution.
  override _.BeginProcessing() = httpClient <- Http.newClient()

  /// Performs clean-up after the command execution.
  override _.EndProcessing() = match httpClient with null -> () | value -> value.Dispose()

  /// Performs execution of this command.
  override this.ProcessRecord() =
    let destinationPath = this.GetUnresolvedProviderPathFromPSPath this.DestinationPath
    if not (Security.canWriteTo destinationPath) then
      let ex = UnauthorizedAccessException "You must run this command in an elevated prompt."
      ErrorRecord(ex, "UnauthorizedAccess", ErrorCategory.PermissionDenied, destinationPath) |> this.ThrowTerminatingError

    let version = this.FetchLatestVersion().GetAwaiter().GetResult()

    let executable = Path.Join(destinationPath, if OperatingSystem.IsWindows() then "node.exe" else "bin/node")
    if not (OperatingSystem.IsWindows()) then IO.makeFileExecutable executable
    executable |> Diagnostics.getExecutableVersion |> this.WriteObject

  /// Downloads the PHP release corresponding to the specified version.
  /// Returns the path to the downloaded archive.
  member private this.DownloadArchive(version: Version): Task<FileInfo> = task {
    let operatingSystem, fileExtension =
      if OperatingSystem.IsMacOS() then "darwin", "tar.gz"
      elif OperatingSystem.IsWindows() then "win", "zip"
      else "linux", "tar.xz"

    let file = $"node-v{version}-{operatingSystem}-x64.{fileExtension}"
    InformationRecord($@"Downloading file ""{file}""...", nameof this.DownloadArchive) |> this.WriteInformation

    let! bytes = (nonNull httpClient).GetByteArrayAsync $"https://nodejs.org/dist/v{version}/{file}"
    let tempFile = Path.GetTempFileName()
    do! File.WriteAllBytesAsync(tempFile, bytes)
    return FileInfo tempFile
  }

  /// Fetches the latest version number of Node.js releases.
  member private this.FetchLatestVersion(): Task<Version> = task {
    InformationRecord("Fetching the list of Node.js releases...", nameof this.FetchLatestVersion) |> this.WriteInformation
    match! (nonNull httpClient).GetFromJsonAsync<List<NodeRelease>> "https://nodejs.org/dist/index.json" with
    | null -> return raise (HttpRequestException "Unable to fetch the list of Node.js releases.")
    | releases -> return releases |> Seq.head |> fun release -> Version.Parse release.Version[1..]
  }
