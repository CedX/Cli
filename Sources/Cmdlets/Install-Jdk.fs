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
  member val DestinationPath = if Platform.IsWindows then @"C:\Program Files\OpenJDK" else "/opt/openjdk" with get, set

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

    let archive = this.DownloadArchive(this.Version).GetAwaiter().GetResult()
    let executable = Path.Join(destinationPath, if Platform.IsWindows then @"bin\java.exe" else "bin/java")

    InformationRecord($@"Extracting the archive into directory ""{destinationPath}""...", nameof this.ProcessRecord) |> this.WriteInformation
    match archive.Extension with
    | ".zip" -> Compression.ExpandZipArchive(archive.FullName, destinationPath, skip = 1)
    | _ -> Compression.ExpandTarArchive(archive.FullName, destinationPath, skip = 1)

    if not Platform.IsWindows then IO.makeFileExecutable executable
    executable |> Diagnostics.getExecutableVersion |> this.WriteObject

  /// Downloads the OpenJDK release corresponding to the specified version.
  /// Returns the path to the downloaded archive.
  member private this.DownloadArchive(version: int): Task<FileInfo> = task {
    let platform, extension =
      if Platform.IsMacOS then "macOS", ".tar.gz"
      elif Platform.IsWindows then "windows", ".zip"
      else "linux", ".tar.gz"

    let file = $"microsoft-jdk-{version}-{platform}-x64{extension}"
    InformationRecord($@"Downloading the file ""{file}""...", nameof this.DownloadArchive) |> this.WriteInformation

    let! bytes = (nonNull httpClient).GetByteArrayAsync $"https://aka.ms/download-jdk/{file}"
    let tempFile = $"{Path.GetTempFileName()}{extension}"
    do! File.WriteAllBytesAsync(tempFile, bytes)
    return FileInfo tempFile
  }
