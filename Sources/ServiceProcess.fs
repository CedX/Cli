namespace Belin.Cli

open System.IO
open System.Management.Automation
open System.ServiceProcess

/// Contains operations for working with service applications.
module ServiceProcess =

  /// The path to the resource directory.
  let private resources: string =
    typeof<Architecture>.Assembly.Location |> Path.GetDirectoryName |> fun name -> Path.Join(name, "../Resources/ServiceProcess")

  /// Returns th absolute path of the `nssm` program according to the specified process architecture.
  let getNssmPath (architecture: Architecture) =
    Path.Join(resources, $"nssm.{(string architecture).ToLowerInvariant()}.exe") |> Path.GetFullPath

  /// Starts the service managed by the specified controller.
  let startService (serviceController: ServiceController) (cmdlet: Cmdlet) =
    if serviceController.Status = ServiceControllerStatus.Stopped then
      InformationRecord($"Starting the {serviceController.ServiceName} service...", "ServiceProcess.startService") |> cmdlet.WriteInformation
      serviceController.Start()
      serviceController.WaitForStatus ServiceControllerStatus.Running

  /// Stops the service managed by the specified controller.
  let stopService (serviceController: ServiceController) (cmdlet: Cmdlet) =
    if serviceController.Status = ServiceControllerStatus.Running then
      InformationRecord($"Stopping the {serviceController.ServiceName} service...", "ServiceProcess.stopService") |> cmdlet.WriteInformation
      serviceController.Stop()
      serviceController.WaitForStatus ServiceControllerStatus.Stopped
