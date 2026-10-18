namespace Belin.Cli

open System
open System.IO
open System.Management.Automation
open System.Net.Http

/// Downloads and installs the latest OpenJDK release.
/// Returns the output from the `java --version` command.
[<Cmdlet(VerbsLifecycle.Install, "Jdk"); OutputType(typeof<string>)>]
type InstallJdk() =
  inherit Cmdlet()

  /// The HTTP client used to download the archive.
  let mutable httpClient: HttpClient | null = null

  /// The path to the output directory.
  [<Parameter(Position = 1); ValidatePath("The specified output path is invalid.")>]
  member val DestinationPath = if OperatingSystem.IsWindows() then @"C:\Program Files\OpenJDK" else "/opt/openjdk"
    with get, set

  /// The major version of the Java development kit.
  [<Parameter; ValidateSet("11", "17", "21", "25")>]
  member val Version = 25 with get, set

  /// Performs initialization of command execution.
  override _.BeginProcessing() = httpClient <- Http.newClient()

  /// Performs clean-up after the command execution.
  override _.EndProcessing() = match httpClient with null -> () | value -> value.Dispose()

  /// Performs execution of this command.
  override this.ProcessRecord() =
    let commandName = "Install-Jdk"
    if not (Security.isPrivilegedProcess (Some this.DestinationPath)) then
      let ex = UnauthorizedAccessException "You must run this command in an elevated prompt."
      ErrorRecord(ex, "UnauthorizedAccess", ErrorCategory.PermissionDenied, this.DestinationPath) |> this.ThrowTerminatingError

    let platform, extension =
      if OperatingSystem.IsMacOS() then "macOS", "tar.gz"
      elif OperatingSystem.IsWindows() then "windows", "zip"
      else "linux", "tar.gz"

    let file = $"microsoft-jdk-{this.Version}-{platform}-x64.{extension}"

    InformationRecord($"""Downloading file "{file}"...""", commandName) |> this.WriteInformation
    let bytes = (nonNull httpClient).GetByteArrayAsync($"https://aka.ms/download-jdk/{file}").GetAwaiter().GetResult()
    let outputFile = Path.GetTempFileName()
    File.WriteAllBytes(outputFile, bytes)

    InformationRecord($"""Extracting file "{file}" into directory "{this.DestinationPath}"...""", commandName) |> this.WriteInformation
    match extension with
    | "zip" -> Compression.expandZipArchive outputFile this.DestinationPath (Some 1)
    | _ -> Compression.expandTarArchive outputFile this.DestinationPath (Some 1)

    let executable = if OperatingSystem.IsWindows() then "java.exe" else "java"
    // let path22 = Path.Join(Path.GetTempPath(), file)
    // File.WriteAllBytes(path, bytes)
    // FileInfo(path)


    ()

/// Downloads and installs the latest Node.js release.
/// Returns the output from the `node --version` command.
[<Cmdlet(VerbsLifecycle.Install, "Node"); OutputType(typeof<string>)>]
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
    let commandName = "Install-Node"
    ()

/// Downloads and installs the latest PHP release.
/// Returns the output from the `php --version` command.
[<Cmdlet(VerbsLifecycle.Install, "Php"); OutputType(typeof<string>)>]
type InstallPhp() =
  inherit Cmdlet()

  /// The HTTP client used to download the archive.
  let mutable httpClient: HttpClient | null = null

  /// The path to the output directory.
  [<Parameter(Position = 1); ValidatePath("The specified output path is invalid.")>]
  member val DestinationPath = @"C:\Program Files\PHP" with get, set

  /// Value indicating whether to register the PHP interpreter with the event log.
  [<Parameter()>]
  member val RegisterEventSource = SwitchParameter(isPresent = false) with get, set

  /// Performs initialization of command execution.
  override _.BeginProcessing() = httpClient <- Http.newClient()

  /// Performs clean-up after the command execution.
  override _.EndProcessing() = match httpClient with null -> () | value -> value.Dispose()

  /// Performs execution of this command.
  override this.ProcessRecord() =
    let commandName = "Install-Php"
    if not (OperatingSystem.IsWindows()) then
      let ex = PlatformNotSupportedException "This command only supports the Windows platform."
      ErrorRecord(ex, "PlatformNotSupported", ErrorCategory.InvalidOperation, Environment.OSVersion) |> this.ThrowTerminatingError

    if not (Security.isPrivilegedProcess (Some this.DestinationPath)) then
      let ex = UnauthorizedAccessException "You must run this command in an elevated prompt."
      ErrorRecord(ex, "UnauthorizedAccess", ErrorCategory.PermissionDenied, this.DestinationPath) |> this.ThrowTerminatingError

    InformationRecord("Fetching the list of PHP releases...", commandName) |> this.WriteInformation
    ()
