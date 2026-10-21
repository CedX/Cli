namespace Belin.Cli

open System
open System.Collections
open System.IO
open System.Management.Automation
open System.ServiceProcess
open System.Text.Json
open System.Xml
open System.Xml.Serialization

/// Provides information about a web application.
[<XmlRoot("Configuration")>]
type ApplicationManifest() =

  /// The application description.
  member val Description = "" with get, set

  /// The application environment.
  member val Environment = "" with get, set

  /// The application identifier.
  member val Id = "" with get, set

  /// The application name.
  member val Name = "" with get, set

  /// Reads the application manifest located at the specified path.
  static member Read(path: string): ApplicationManifest =
    let emptyManifest = ApplicationManifest()
    match (nonNull (Path.GetExtension path)).ToLowerInvariant() with
    | ".config" | ".xml" ->
      use xmlReader = XmlReader.Create path
      XmlSerializer(typeof<ApplicationManifest>).Deserialize xmlReader :?> ApplicationManifest | null |> defaultIfNull emptyManifest
    | ".json" ->
      JsonSerializer.Deserialize<ApplicationManifest>(File.ReadAllText path, JsonSerializerOptions.Web) |> defaultIfNull emptyManifest
    | ".psd1" ->
      path |> Automation.parsePowerShellDataFile |> ApplicationManifest.OfDictionary
    | _ ->
      emptyManifest

  /// Creates a new application manifest from the specified dictionary.
  static member private OfDictionary(dictionary: IDictionary) = ApplicationManifest(
    Description = (match dictionary["Description"] with :? string as value -> value | _ -> ""),
    Environment = (match dictionary["Environment"] with :? string as value -> value | _ -> ""),
    Id = (match dictionary["Id"] with :? string as value -> value | _ -> ""),
    Name = (match dictionary["Name"] with :? string as value -> value | _ -> "")
  )

/// Represents a service application.
[<AbstractClass>]
type Service(path: string) as this =
  do
    this.Path <- path |> Path.GetFullPath |> Path.TrimEndingDirectorySeparator
    let manifestPath =
      ["Sources/Server", "Sources"]
      |> Seq.collect (fun folder -> ["config"; "json"; "psd1"; "xml"] |> List.map (fun ext -> Path.Join(this.Path, $"{folder}/appsettings.{ext}")))
      |> Seq.filter File.Exists
      |> Seq.exactlyOne

    this.Manifest <- ApplicationManifest.Read manifestPath
    if String.IsNullOrWhiteSpace this.Manifest.Id then raise (EntryPointNotFoundException "Unable to locate the service manifest.")

  /// The entry point of this service.
  abstract EntryPoint: string with get

  /// The name of the environment variable storing the service environment.
  abstract EnvironmentVariable: string with get

  /// Value indicating whether the service uses a 32-bit process.
  member val Is32Bit = not Environment.Is64BitOperatingSystem with get, set

  /// The service manifest.
  member val Manifest: ApplicationManifest = ApplicationManifest() with get, set

  /// The path to the service root directory.
  member val Path = path with get, set

  /// The program used to run this service.
  abstract Program: string with get

/// Contains operations for working with service applications.
module ServiceProcess =

  /// The path to the resource directory.
  let private resources: string =
    typeof<Architecture>.Assembly.Location |> Path.GetDirectoryName |> fun name -> Path.Join(name, "../Resources/ServiceProcess")

  /// Returns th absolute path of the `nssm` program according to the specified process architecture.
  let getNssmPath (architecture: Architecture): string =
    Path.GetFullPath($"nssm.{(string architecture).ToLowerInvariant()}.exe", resources)

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
