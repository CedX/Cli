namespace Belin.Cli.Cmdlets

open Belin.Cli
open System
open System.Collections.Generic
open System.IO
open System.Management.Automation
open System.Net.Http
open System.Net.Http.Json
open System.ServiceProcess
open System.Threading.Tasks

/// Represents a Gitea release.
type GiteaRelease = {

  /// The version number.
  Version: string
}

/// Downloads and installs the latest Gitea release.
/// Returns the output from the `gitea --version` command.
[<Cmdlet(VerbsLifecycle.Install, "Gitea"); OutputType(typeof<string>)>]
type InstallGitea() =
  inherit PSCmdlet()

  /// The HTTP client used to download the archive.
  let mutable httpClient: HttpClient | null = null

  /// The path to the output directory.
  [<Parameter(Position = 1); ValidatePath("The specified output path is invalid.")>]
  member val DestinationPath = if Platform.IsWindows then @"C:\Gitea" else "/opt/gitea" with get, set

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
    let binary = this.DownloadBinary(version).GetAwaiter().GetResult()
    let executable = Path.Join(destinationPath, if Platform.IsWindows then "gitea.exe" else "gitea")

    use serviceController = new ServiceController "Gitea"
    if Environment.IsPrivilegedProcess then this |> ServiceProcess.stopService serviceController

    InformationRecord($@"Moving the binary into directory ""{destinationPath}""...", nameof this.ProcessRecord) |> this.WriteInformation
    Directory.CreateDirectory destinationPath |> ignore<DirectoryInfo>
    File.Copy(binary.FullName, executable, overwrite = true)
    if not Platform.IsWindows then File.makeExecutable executable

    if Environment.IsPrivilegedProcess then this |> ServiceProcess.startService serviceController
    executable |> Diagnostics.getExecutableVersion |> this.WriteObject

  /// Downloads the Gitea release corresponding to the specified version.
  /// Returns the path to the downloaded binary.
  member private this.DownloadBinary(version: Version): Task<FileInfo> = task {
    let platform, extension =
      if Platform.IsMacOS then "darwin", ""
      elif Platform.IsWindows then "windows", ".exe"
      else "linux", ""

    let file = $"gitea-{version}-{platform}-amd64{extension}"
    InformationRecord($@"Downloading the file ""{file}""...", nameof this.DownloadBinary) |> this.WriteInformation

    let! bytes = (nonNull httpClient).GetByteArrayAsync $"https://dl.gitea.com/gitea/{version}/{file}"
    let tempFile = $"{Path.GetTempFileName()}{extension}"
    do! File.WriteAllBytesAsync(tempFile, bytes)
    return FileInfo tempFile
  }

  /// Fetches the latest version number of Gitea releases.
  member private this.FetchLatestVersion(): Task<Version> = task {
    InformationRecord("Fetching the list of Gitea releases...", nameof this.FetchLatestVersion) |> this.WriteInformation
    match! (nonNull httpClient).GetFromJsonAsync<Dictionary<string, GiteaRelease>> "https://dl.gitea.com/gitea/version.json" with
    | null -> return raise (HttpRequestException "Unable to fetch the list of Gitea releases.")
    | releases -> return releases |> Seq.head |> fun release -> Version.Parse release.Value.Version
  }
