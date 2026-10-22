namespace Belin.Cli.Cmdlets

open Belin.Cli
open Microsoft.Win32
open System
open System.Collections.Generic
open System.IO
open System.Management.Automation
open System.Net.Http
open System.Net.Http.Json
open System.ServiceProcess
open System.Threading.Tasks

/// Represents a PHP release.
type PhpRelease = {

  /// The version number.
  Version: string
}

/// Downloads and installs the latest PHP release.
/// Returns the output from the `php --version` command.
[<Cmdlet(VerbsLifecycle.Install, "Php"); OutputType(typeof<string>); Sealed>]
type InstallPhp() =
  inherit PSCmdlet()

  /// The HTTP client used to download the archive.
  let mutable httpClient: HttpClient | null = null

  /// The path to the output directory.
  [<Parameter(Position = 1); ValidatePath("The specified output path is invalid.")>]
  member val DestinationPath = if Platform.IsWindows then @"C:\Program Files\PHP" else "/usr/local" with get, set

  /// Value indicating whether to register the PHP interpreter with the event log.
  [<Parameter()>]
  member val RegisterEventSource = SwitchParameter(isPresent = false) with get, set

  interface IDisposable with
    /// Releases any resources associated with this object.
    member _.Dispose() = match httpClient with null -> () | disposable -> disposable.Dispose()

  /// Performs initialization of command execution.
  override this.BeginProcessing() =
    httpClient <- Http.newClient()
    if not Platform.IsWindows then
      let ex = PlatformNotSupportedException "This command only supports the Windows platform."
      ErrorRecord(ex, "PlatformNotSupported", ErrorCategory.InvalidOperation, Environment.OSVersion) |> this.ThrowTerminatingError

  /// Performs execution of this command.
  override this.ProcessRecord() =
    let destinationPath = this.GetUnresolvedProviderPathFromPSPath this.DestinationPath
    if not (Security.canWriteTo destinationPath) then
      let ex = UnauthorizedAccessException "You must run this command in an elevated prompt."
      ErrorRecord(ex, "UnauthorizedAccess", ErrorCategory.PermissionDenied, destinationPath) |> this.ThrowTerminatingError

    let version = this.FetchLatestVersion().GetAwaiter().GetResult()
    let archive = this.DownloadArchive(version).GetAwaiter().GetResult()
    let executable = Path.Join(destinationPath, if Platform.IsWindows then "php.exe" else "bin/php")

    use serviceController = new ServiceController "W3SVC"
    if Environment.IsPrivilegedProcess then this |> ServiceProcess.stopService serviceController

    InformationRecord($@"Extracting the archive into directory ""{destinationPath}""...", nameof this.ProcessRecord) |> this.WriteInformation
    Compression.ExpandZipArchive(archive.FullName, destinationPath)
    if not Platform.IsWindows then File.makeExecutable executable

    if Environment.IsPrivilegedProcess then
      this |> ServiceProcess.startService serviceController
      if this.RegisterEventSource.IsPresent then this.RegisterEventLog(version, destinationPath)

    executable |> Diagnostics.getExecutableVersion |> this.WriteObject

  /// Downloads the PHP release corresponding to the specified version.
  /// Returns the path to the downloaded archive.
  member private this.DownloadArchive(version: Version): Task<FileInfo> = task {
    let vs = if version >= Version "8.4.0" then "vs17" else "vs16"
    let architecture = (string Architecture.Current).ToLowerInvariant()
    let file = $"php-{version}-nts-Win32-{vs}-{architecture}.zip"
    InformationRecord($@"Downloading the file ""{file}""...", nameof this.DownloadArchive) |> this.WriteInformation

    let! bytes = (nonNull httpClient).GetByteArrayAsync $"https://downloads.php.net/~windows/releases/archives/{file}"
    let tempFile = $"{Path.GetTempFileName()}.zip"
    do! File.WriteAllBytesAsync(tempFile, bytes)
    return FileInfo tempFile
  }

  /// Fetches the latest version number of PHP releases.
  member private this.FetchLatestVersion(): Task<Version> = task {
    InformationRecord("Fetching the list of PHP releases...", nameof this.FetchLatestVersion) |> this.WriteInformation
    match! (nonNull httpClient).GetFromJsonAsync<Dictionary<string, PhpRelease>> "https://www.php.net/releases/?json" with
    | null -> return raise (HttpRequestException "Unable to fetch the list of PHP releases.")
    | releases -> return releases |> Seq.head |> fun release -> Version.Parse release.Value.Version
  }

  /// Registers the PHP interpreter with the event log.
  member private this.RegisterEventLog(phpVersion: Version, phpDirectory: string) =
    InformationRecord("Registering the PHP interpreter with the event log...", nameof this.RegisterEventLog) |> this.WriteInformation
    let key = $@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\EventLog\Application\PHP-{phpVersion}"
    Registry.SetValue(key, "EventMessageFile", Path.GetFullPath($"php{phpVersion.Major}.dll", phpDirectory), RegistryValueKind.String)
    Registry.SetValue(key, "TypesSupported", 7, RegistryValueKind.DWord)
