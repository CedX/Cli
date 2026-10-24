namespace Belin.Cli.Cmdlets

open Belin.Cli
open Belin.Cli.Services
open System
open System.IO
open System.Management.Automation
open System.ServiceProcess

/// Registers a Windows service based on [NSSM](https://nssm.cc).
[<Cmdlet(VerbsCommon.New, "NssmService"); OutputType(typeof<Void>)>]
type NewNssmService() =
  inherit PSCmdlet()

  /// Gets application information for the specified command.
  let getApplicationInfo (command: string): ApplicationInfo option =
    ScriptBlock.Create("Get-Command $_").Invoke [| command |]
    |> Seq.map (fun psObject -> psObject.BaseObject :?> ApplicationInfo)
    |> Seq.tryHead

  /// The absolute path to the NSSM executable.
  let nssmPath =
    getApplicationInfo "nssm"
    |> Option.map (fun applicationInfo -> applicationInfo.Path)
    |> Option.defaultValue (Nssm.getPath Architecture.Current)

  /// The path to the root directory of the web application.
  [<Parameter(Position = 1, ValueFromPipeline = true); ValidateDirectory("The specified directory does not exist.")>]
  member val Path = "" with get, set

  /// The account used by the service as the logon account.
  [<Parameter; Credential>]
  member val Credential: PSCredential | null = null with get, set

  /// Value indicating whether to start the service after its registration.
  [<Parameter>]
  member val Start = SwitchParameter false with get, set

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
    if ServiceProcess.serviceExists service.Manifest.Id then
      let ex = InvalidOperationException $@"The service ""{service.Manifest.Id}"" already exists."
      ErrorRecord(ex, "InvalidOperation", ErrorCategory.InvalidOperation, service) |> this.WriteError

    let properties = dict [
      "AppDirectory", service.Path
      "AppEnvironmentExtra", $"{service.EnvironmentVariable}={service.Manifest.Environment}"
      "AppNoConsole", "1"
      "AppStderr", Path.Join(service.Path, @"Temp\Error.log")
      "AppStdout", Path.Join(service.Path, @"Temp\Output.log")
      "Description", service.Manifest.Description
      "DisplayName", service.Manifest.Name
      "Start", "SERVICE_AUTO_START"
    ]

    match getApplicationInfo service.Program with
    | None ->
      let ex = FileNotFoundException $@"The program ""{service.Program}"" cannot be found."
      ErrorRecord(ex, "FileNotFound", ErrorCategory.ObjectNotFound, service.Program) |> this.WriteError
    | Some applicationInfo ->
      let mutable programPath = applicationInfo.Path
      if Platform.IsWindows && Environment.Is64BitOperatingSystem && service.Is32Bit then
        programPath <- programPath.Replace(@"\Program Files\", @"\Program Files (x86)\")

      Nssm.run nssmPath "install" service.Manifest.Id [programPath; service.EntryPoint]
      for entry in properties do Nssm.run nssmPath "set" service.Manifest.Id [entry.Key; entry.Value]

      if this.Start.IsPresent then
        use serviceController = new ServiceController(service.Manifest.Id)
        this |> ServiceProcess.startService serviceController

      match this.Credential with
      | null -> ()
      | credential ->
        let password = if credential.Password.Length > 0 then credential.GetNetworkCredential().Password else ""
        Nssm.run nssmPath "set" service.Manifest.Id ["ObjectName"; credential.UserName; password]

      let created = if this.Start.IsPresent then "started" else "created"
      InformationRecord($@"The service ""{service.Manifest.Id}"" has been successfully {created}.", nameof this.ProcessRecord) |> this.WriteInformation
