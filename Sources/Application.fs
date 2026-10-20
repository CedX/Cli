namespace Belin.Cli

open System
open System.Collections
open System.Collections.Generic
open System.IO
open System.Management.Automation
open System.Management.Automation.Language
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
      ApplicationManifest.ParsePowerShellDataFile path |> ApplicationManifest.OfDictionary
    | _ ->
      emptyManifest

  /// Creates a new application manifest from the specified dictionary.
  static member private OfDictionary(dictionary: IDictionary) = ApplicationManifest(
    Description = (match dictionary["Description"] with :? string as value -> value | _ -> ""),
    Environment = (match dictionary["Environment"] with :? string as value -> value | _ -> ""),
    Id = (match dictionary["Id"] with :? string as value -> value | _ -> ""),
    Name = (match dictionary["Name"] with :? string as value -> value | _ -> "")
  )

  /// Parses the PowerShell data file located at the specified path.
  static member private ParsePowerShellDataFile(path: string): Hashtable =
    let scriptBlockAst, _, errors = Parser.ParseFile path
    if not (Array.isEmpty errors) then raise (FormatException (errors |> Array.map _.Message |> String.concat Environment.NewLine))
    match scriptBlockAst.Find((fun ast -> ast :? HashtableAst), searchNestedScriptBlocks = false) with
    | :? HashtableAst as hashtableAst -> hashtableAst.SafeGetValue() :?> Hashtable
    | _ -> raise (FormatException "The PowerShell data file is invalid.")

/// Represents a web application.
[<AbstractClass>]
type Application(path: string) as this =
  do
    this.Path <- path |> Path.GetFullPath |> Path.TrimEndingDirectorySeparator
    let configFile =
      ["Sources/Server", "Sources"]
      |> List.collect (fun folder -> ["config"; "json"; "psd1"; "xml"] |> List.map (fun ext -> Path.Join(this.Path, $"{folder}/appsettings.{ext}")))
      |> List.filter File.Exists
      |> List.tryHead

    this.Manifest <-
      match configFile with
      | None -> ApplicationManifest()
      | Some filePath -> ApplicationManifest.Read filePath

    if String.IsNullOrWhiteSpace this.Manifest.Id then raise (EntryPointNotFoundException "Unable to locate the application manifest.")

  /// The entry point of this application.
  abstract EntryPoint: string with get

  /// The name of the environment variable storing the application environment.
  abstract EnvironmentVariable: string with get

  /// Value indicating whether the application uses a 32-bit process.
  member val Is32Bit = not Environment.Is64BitOperatingSystem with get, set

  /// The application manifest.
  member val Manifest: ApplicationManifest = ApplicationManifest() with get, set

  /// The path to the application root directory.
  member val Path = path with get, set

  /// The program used to run this application.
  abstract Program: string with get

/// Represents a .NET application.
type DotNetApplication(path: string) =
  inherit Application(path)

  /// The path of the application entry point.
  let mutable entryPath = ""

  /// The entry point of this application.
  override val EntryPoint =
    if entryPath.Length > 0 then entryPath
    else raise (EntryPointNotFoundException "Unable to resolve the application entry point.")

  /// The name of the environment variable storing the application environment.
  override val EnvironmentVariable = "DOTNET_ENVIRONMENT"

  /// The program used to run this application.
  override val Program = if Platform.IsWindows then "dotnet.exe" else "dotnet"

/// Represents the contents of a `package.json` file.
type NodePackage = {

  /// The dictionary of package commands.
  Bin: Dictionary<string, string> option

  /// The package description.
  Description: string

  /// The package name.
  Name: string
}

/// Represents a Node.js application.
type NodeApplication(path: string) as this =
  inherit Application(path)

  /// The path of the application entry point.
  let mutable entryPath = ""

  do
    let packagePath = Path.Join(this.Path, "package.json")
    let packageManifest =
      match File.Exists packagePath with
      | false -> None
      | true ->
        let package = JsonSerializer.Deserialize<NodePackage>(File.ReadAllText packagePath, JsonSerializerOptions.Web)
        Some (package |> defaultIfNull { Bin = None; Description = ""; Name = "" })

    match packageManifest with
    | None -> ()
    | Some package ->
      if String.IsNullOrWhiteSpace this.Manifest.Description then this.Manifest.Description <- package.Description
      if String.IsNullOrWhiteSpace this.Manifest.Name then this.Manifest.Name <- package.Name
      match package.Bin with
      | None -> ()
      | Some dictionary when dictionary.Count = 0 -> ()
      | Some dictionary -> entryPath <- Path.GetFullPath(dictionary |> Seq.head |> _.Value, this.Path)

  /// The entry point of this application.
  override val EntryPoint =
    if entryPath.Length > 0 then entryPath
    else raise (EntryPointNotFoundException "Unable to resolve the application entry point.")

  /// The name of the environment variable storing the application environment.
  override val EnvironmentVariable = "NODE_ENV"

  /// The program used to run this application.
  override val Program = if Platform.IsWindows then "node.exe" else "node"

/// Represents a PowerShell application.
type PowerShellApplication(path: string) as this =
  inherit Application(path)

  /// The path of the application entry point.
  let mutable entryPath = ""

  do
    let moduleManifest =
      Directory.EnumerateFiles(this.Path, "*.psd1")
      |> Seq.filter (fun file ->
        let name = Path.GetFileNameWithoutExtension file
        name <> "PSModules" && name <> "PSScriptAnalyzerSettings")
      |> Seq.tryHead

    match moduleManifest with
    | None -> ()
    | Some filePath ->
      let manifest =
        ScriptBlock.Create($"Import-PowerShellDataFile '{filePath}'").Invoke()
        |> Seq.head
        |> fun psObject -> psObject.BaseObject :?> IDictionary

      let rootModule = string manifest["RootModule"]
      if String.IsNullOrWhiteSpace this.Manifest.Description then this.Manifest.Description <- string manifest["Description"]
      if String.IsNullOrWhiteSpace this.Manifest.Name then this.Manifest.Name <- Path.GetFileNameWithoutExtension filePath |> defaultIfNull ""
      if not (String.IsNullOrWhiteSpace rootModule) then entryPath <- Path.GetFullPath(rootModule, this.Path)

  /// The entry point of this application.
  override val EntryPoint =
    if entryPath.Length > 0 then $@"-ExecutionPolicy Bypass -File ""{entryPath}"" -NoLogo -NoProfile -NonInteractive"
    else raise (EntryPointNotFoundException "Unable to resolve the application entry point.")

  /// The name of the environment variable storing the application environment.
  override val EnvironmentVariable = "PODE_ENVIRONMENT"

  /// The program used to run this application.
  override val Program = if Platform.IsWindows then "pwsh.exe" else "pwsh"
