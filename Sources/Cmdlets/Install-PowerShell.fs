namespace Belin.Cli.Cmdlets

open Belin.Cli
open System
open System.IO
open System.Management.Automation
open System.Net.Http
open System.Net.Http.Json
open System.Threading.Tasks

/// Represents a PowerShell release.
type PowerShellRelease = {

  /// The Git tag of the current stable release.
  StableReleaseTag: string
}

/// Downloads and installs the latest PowerShell release.
/// Returns the output from the `pwsh --version` command.
[<Cmdlet(VerbsLifecycle.Install, "PowerShell"); OutputType(typeof<string>); Sealed>]
type InstallPowerShell() =
  inherit PSCmdlet()

  /// The HTTP client used to download the archive.
  let mutable httpClient: HttpClient | null = null

  /// The path to the output directory.
  [<Parameter(Position = 1); ValidatePath("The specified output path is invalid.")>]
  member val DestinationPath = if Platform.IsWindows then @"C:\Program Files\PowerShell" else "/opt/powershell" with get, set

  interface IDisposable with
    /// Releases any resources associated with this object.
    member _.Dispose() = match httpClient with null -> () | disposable -> disposable.Dispose()

  /// Performs initialization of command execution.
  override _.BeginProcessing() = httpClient <- Http.newClient()

  /// Performs execution of this command.
  override this.ProcessRecord() =
    let destinationPath = this.GetUnresolvedProviderPathFromPSPath this.DestinationPath
    if not (Security.canWriteTo destinationPath) then
      let ex = UnauthorizedAccessException "You must run this command in an elevated prompt."
      ErrorRecord(ex, "UnauthorizedAccess", ErrorCategory.PermissionDenied, destinationPath) |> this.ThrowTerminatingError

    let version = this.FetchLatestVersion().GetAwaiter().GetResult()
    let archive = this.DownloadArchive(version).GetAwaiter().GetResult()
    let executable = Path.Join(destinationPath, if Platform.IsWindows then "pwsh.exe" else "pwsh")

    InformationRecord($@"Extracting the archive into the directory ""{destinationPath}""...", nameof this.ProcessRecord) |> this.WriteInformation
    match archive.Extension with
    | ".zip" -> Compression.ExpandZipArchive(archive.FullName, destinationPath)
    | _ -> Compression.ExpandTarArchive(archive.FullName, destinationPath)

    executable |> Diagnostics.getExecutableVersion |> this.WriteObject

  /// Downloads the PowerShell release corresponding to the specified version.
  /// Returns the path to the downloaded archive.
  member private this.DownloadArchive(version: Version): Task<FileInfo> = task {
    let platform, architecture, extension =
      if Platform.IsMacOS then "osx", "arm64", "tar.gz"
      elif Platform.IsWindows then "win", "x64", "zip"
      else "linux", "x64", "tar.gz"

    let file = $"powershell-{version}-{platform}-{architecture}.{extension}"
    InformationRecord($@"Downloading the file ""{file}""...", nameof this.DownloadArchive) |> this.WriteInformation

    let! bytes = (nonNull httpClient).GetByteArrayAsync $"https://github.com/PowerShell/PowerShell/releases/download/v{version}/{file}"
    let tempFile = $"{Path.GetTempFileName()}{extension}"
    do! File.WriteAllBytesAsync(tempFile, bytes)
    return FileInfo tempFile
  }

  /// Fetches the latest version number of PowerShell releases.
  member private this.FetchLatestVersion(): Task<Version> = task {
    InformationRecord("Fetching the list of PowerShell releases...", nameof this.FetchLatestVersion) |> this.WriteInformation
    let metadataUrl = "https://raw.githubusercontent.com/PowerShell/PowerShell/master/tools/metadata.json"
    match! (nonNull httpClient).GetFromJsonAsync<PowerShellRelease> metadataUrl with
    | null -> return raise (HttpRequestException "Unable to fetch the list of PowerShell releases.")
    | release -> return Version.Parse release.StableReleaseTag[1..]
  }
