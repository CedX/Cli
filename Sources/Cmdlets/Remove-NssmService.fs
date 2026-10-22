namespace Belin.Cli.Cmdlets

open Belin.Cli
open Belin.Cli.Services
open System
open System.Management.Automation
open System.ServiceProcess

/// Unregisters a Windows service based on [NSSM](https://nssm.cc).
[<Cmdlet(VerbsCommon.Remove, "NssmService"); OutputType(typeof<Void>)>]
type RemoveNssmService() =
  inherit PSCmdlet()

  /// The path to the root directory of the web application.
  [<Parameter(Position = 1, ValueFromPipeline = true); ValidateDirectory("The specified directory does not exist.")>]
  member val Path = "" with get, set

  /// Performs initialization of command execution.
  override this.BeginProcessing() =
    if not Platform.IsWindows then
      let ex = PlatformNotSupportedException "This command only supports the Windows platform."
      ErrorRecord(ex, "PlatformNotSupported", ErrorCategory.InvalidOperation, Environment.OSVersion) |> this.ThrowTerminatingError

    if not Environment.IsPrivilegedProcess then
      let ex = UnauthorizedAccessException "You must run this command in an elevated prompt."
      ErrorRecord(ex, "UnauthorizedAccess", ErrorCategory.PermissionDenied, Environment.UserName) |> this.ThrowTerminatingError

  /// Performs execution of this command.
  override this.ProcessRecord() =
    if String.IsNullOrWhiteSpace this.Path then this.Path <- this.SessionState.Path.CurrentLocation.Path

    let service = ServiceFactory.newService (this.GetUnresolvedProviderPathFromPSPath this.Path)
    if not (ServiceProcess.serviceExists service.Manifest.Id) then
      let ex = InvalidOperationException $@"The service ""{service.Manifest.Id}"" does not exist."
      ErrorRecord(ex, "InvalidOperation", ErrorCategory.ObjectNotFound, service) |> this.WriteError

    use serviceController = new ServiceController(service.Manifest.Id)
    this |> ServiceProcess.stopService serviceController
    Nssm.remove service
    InformationRecord($@"The service ""{service.Manifest.Id}"" has been successfully removed.", nameof this.ProcessRecord) |> this.WriteInformation
