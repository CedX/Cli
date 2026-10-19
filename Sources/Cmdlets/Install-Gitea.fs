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

    use serviceController = new ServiceController "Gitea"
    if Environment.IsPrivilegedProcess then this.StopService serviceController

    if Environment.IsPrivilegedProcess then this.StartService serviceController

    let executable = Path.Join(destinationPath, if Platform.IsWindows then "gitea.exe" else "gitea")
    if not Platform.IsWindows then IO.makeFileExecutable executable
    executable |> Diagnostics.getExecutableVersion |> this.WriteObject

  /// Downloads the Gitea release corresponding to the specified version.
  /// Returns the path to the downloaded archive.
  member private this.DownloadArchive(version: Version): Task<FileInfo> = task {
    let vs = if version >= Version "8.4.0" then "vs17" else "vs16"
    let architecture = (string Architecture.Current).ToLowerInvariant()
    let file = $"php-{version}-nts-Win32-{vs}-{architecture}.zip"
    InformationRecord($@"Downloading the file ""{file}""...", nameof this.DownloadArchive) |> this.WriteInformation

    let! bytes = (nonNull httpClient).GetByteArrayAsync $"https://downloads.php.net/~windows/releases/archives/{file}"
    let tempFile = $"{Path.GetTempFileName()}{Path.GetExtension file}"
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

  /// Starts the Gitea service.
  member private this.StartService(serviceController: ServiceController) =
    if serviceController.Status = ServiceControllerStatus.Stopped then
      InformationRecord("Starting the Gitea service...", nameof this.StartService) |> this.WriteInformation
      serviceController.Start()
      serviceController.WaitForStatus ServiceControllerStatus.Running

  /// Stops the Gitea service.
  member private this.StopService(serviceController: ServiceController) =
    if serviceController.Status = ServiceControllerStatus.Running then
      InformationRecord("Stopping the Gitea service...", nameof this.StopService) |> this.WriteInformation
      serviceController.Stop()
      serviceController.WaitForStatus ServiceControllerStatus.Stopped
