namespace Belin.Cli.Cmdlets

open Belin.Cli
open System
open System.Management.Automation
open System.Net.Http

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
