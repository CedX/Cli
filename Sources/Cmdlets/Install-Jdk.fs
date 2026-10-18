namespace Belin.Cli.Cmdlets

open Belin.Cli
open System
open System.IO
open System.Management.Automation
open System.Net.Http
open System.Threading.Tasks

/// Downloads and installs the latest OpenJDK release.
/// Returns the output from the `java --version` command.
[<Cmdlet(VerbsLifecycle.Install, "Jdk"); OutputType(typeof<string>)>]
type InstallJdk() =
  inherit PSCmdlet()

  /// The HTTP client used to download the archive.
  let mutable httpClient: HttpClient | null = null

  /// The path to the output directory.
  [<Parameter(Position = 1); ValidatePath("The specified output path is invalid.")>]
  member val DestinationPath = if OperatingSystem.IsWindows() then @"C:\Program Files\OpenJDK" else "/opt/openjdk" with get, set

  /// The major version of the Java development kit.
  [<Parameter; ValidateSet("11", "17", "21", "25")>]
  member val Version = 25 with get, set

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

    let platform, extension =
      if OperatingSystem.IsMacOS() then "macOS", "tar.gz"
      elif OperatingSystem.IsWindows() then "windows", "zip"
      else "linux", "tar.gz"

    let file = $"microsoft-jdk-{this.Version}-{platform}-x64.{extension}"

    InformationRecord($"""Downloading file "{file}"...""", nameof this.ProcessRecord) |> this.WriteInformation
    let bytes = (nonNull httpClient).GetByteArrayAsync($"https://aka.ms/download-jdk/{file}").GetAwaiter().GetResult()
    let outputFile = Path.GetTempFileName()
    File.WriteAllBytes(outputFile, bytes)

    InformationRecord($"""Extracting file "{file}" into directory "{destinationPath}"...""", nameof this.ProcessRecord) |> this.WriteInformation
    match extension with
    | "zip" -> Compression.ExpandZipArchive(outputFile, destinationPath, skip = 1)
    | _ -> Compression.ExpandTarArchive(outputFile, destinationPath, skip = 1)

    let executable = Path.Join(destinationPath, if OperatingSystem.IsWindows() then "java.exe" else "bin/java")
    if not (OperatingSystem.IsWindows()) then IO.makeFileExecutable executable
    executable |> Diagnostics.getExecutableVersion |> this.WriteObject

  /// Downloads the PHP release corresponding to the specified version.
  /// Returns the path to the downloaded archive.
  member private this.DownloadArchive(version: Version): Task<FileInfo> = task {
    let vs = if version >= Version "8.4.0" then "vs17" else "vs16"
    let architecture = (string Architecture.Current).ToLowerInvariant()
    let file = $"php-{version}-nts-Win32-{vs}-{architecture}.zip"
    InformationRecord($@"Downloading file ""{file}""...", nameof this.DownloadArchive) |> this.WriteInformation

    let! bytes = (nonNull httpClient).GetByteArrayAsync $"https://downloads.php.net/~windows/releases/archives/{file}"
    let tempFile = Path.GetTempFileName()
    do! File.WriteAllBytesAsync(tempFile, bytes)
    return FileInfo tempFile
  }
