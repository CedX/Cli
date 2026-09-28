namespace Belin.Cli

open System
open System.Management.Automation

/// Downloads and installs the latest OpenJDK release.
/// Returns the output from the `java --version` command.
[<Cmdlet(VerbsLifecycle.Install, "Jdk")>]
[<OutputType(typeof<string>)>]
type InstallJdkCommand() =
  inherit Cmdlet()

  /// The path to the output directory.
  [<Parameter(Position = 1)>]
  member val DestinationPath = if OperatingSystem.IsWindows() then @"C:\Program Files\OpenJDK" else "/opt/openjdk"
    with get, set

  /// The major version of the Java development kit.
  [<Parameter; ValidateSetAttribute("11", "17", "21", "25")>]
  member val Version = 25 with get, set

  /// Performs execution of this command.
  override this.ProcessRecord() =
    if not (Security.isPrivilegedProcess (Some this.DestinationPath)) then
      let ex = UnauthorizedAccessException "You must run this command in an elevated prompt."
      ErrorRecord(ex, "UnauthorizedAccess", ErrorCategory.PermissionDenied, this.DestinationPath) |> this.ThrowTerminatingError

    ()

/// Downloads and installs the latest Node.js release.
/// Returns the output from the `node --version` command.
[<Cmdlet(VerbsLifecycle.Install, "Node")>]
[<OutputType(typeof<string>)>]
type InstallNodeCommand() =
  inherit Cmdlet()

  /// The path to the output directory.
  [<Parameter(Position = 1)>]
  member val DestinationPath = if OperatingSystem.IsWindows() then @"C:\Program Files\Node.js" else "/usr/local"
    with get, set

  /// Performs execution of this command.
  override this.ProcessRecord() =
    ()

/// Downloads and installs the latest PHP release.
/// Returns the output from the `php --version` command.
[<Cmdlet(VerbsLifecycle.Install, "Php")>]
[<OutputType(typeof<string>)>]
type InstallPhpCommand() =
  inherit Cmdlet()

  /// The path to the output directory.
  [<Parameter(Position = 1)>]
  member val DestinationPath = @"C:\Program Files\PHP" with get, set

  /// Value indicating whether to register the PHP interpreter with the event log.
  [<Parameter()>]
  member val RegisterEventSource = SwitchParameter(isPresent = false) with get, set

  /// Performs execution of this command.
  override this.ProcessRecord() =
    if not (OperatingSystem.IsWindows()) then
      let ex = PlatformNotSupportedException "This command only supports the Windows platform."
      ErrorRecord(ex, "PlatformNotSupported", ErrorCategory.InvalidOperation, Environment.OSVersion) |> this.ThrowTerminatingError

    if not (Security.isPrivilegedProcess (Some this.DestinationPath)) then
      let ex = UnauthorizedAccessException "You must run this command in an elevated prompt."
      ErrorRecord(ex, "UnauthorizedAccess", ErrorCategory.PermissionDenied, this.DestinationPath) |> this.ThrowTerminatingError

    InformationRecord("Fetching the list of PHP releases...", "Install-Php") |> this.WriteInformation
    ()
