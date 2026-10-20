namespace Belin.Cli

open System
open System.Collections
open System.IO
open System.Management.Automation
open System.Text.Json
open System.Xml
open System.Xml.Serialization

/// Provides information about a web application.
[<XmlRoot("Configuration")>]
type ApplicationManifest =
  {
    /// The application description.
    Description: string

    /// The application environment.
    Environment: string

    /// The application identifier.
    Id: string

    /// The application name.
    Name: string
  }

  /// Creates a new instance with default values.
  static member Default =
    { Description = ""; Environment = ""; Id = ""; Name = "" }

  /// Creates a new application manifest from the specified dictionary.
  static member OfDictionary(dictionary: IDictionary): ApplicationManifest = {
    Description = match dictionary["Description"] with :? string as value -> value | _ -> ""
    Environment = match dictionary["Environment"] with :? string as value -> value | _ -> ""
    Id = match dictionary["Id"] with :? string as value -> value | _ -> ""
    Name = match dictionary["Name"] with :? string as value -> value | _ -> ""
  }

  /// Reads the application manifest located at the specified path.
  static member Read(path: string): ApplicationManifest =
    match (nonNull (Path.GetExtension path)).ToLowerInvariant() with
    | ".config" | ".xml" ->
      use xmlReader = XmlReader.Create path
      XmlSerializer(typeof<ApplicationManifest>).Deserialize xmlReader :?> ApplicationManifest | null |> defaultIfNull ApplicationManifest.Default
    | ".json" ->
      JsonSerializer.Deserialize<ApplicationManifest>(File.ReadAllText path, JsonSerializerOptions.Web) |> defaultIfNull ApplicationManifest.Default
    | ".psd1" ->
      let scriptBlock = ScriptBlock.Create $"Import-PowerShellDataFile '{path}'"
      scriptBlock.Invoke() |> Seq.head |> fun psObject -> psObject.BaseObject :?> IDictionary |> ApplicationManifest.OfDictionary
    | _ ->
      ApplicationManifest.Default

/// Represents a web application.
[<AbstractClass>]
type Application(path: string) as this =
  do
    this.Path <- path |> Path.GetFullPath |> Path.TrimEndingDirectorySeparator
    let configFile =
      ["Sources/Server", "Sources"]
      |> List.collect (fun folder -> ["config"; "json"; "psd1"; "xml"] |> List.map (fun extension -> Path.Join(this.Path, $"{folder}/appsettings.{extension}")))
      |> List.filter File.Exists
      |> List.tryHead

    this.Manifest <-
      match configFile with
      | None -> ApplicationManifest.Default
      | Some filePath -> ApplicationManifest.Read filePath

    if String.IsNullOrWhiteSpace this.Manifest.Id then raise (EntryPointNotFoundException "Unable to locate the application manifest.")

  /// The entry point of this application.
  abstract EntryPoint: string with get

  /// The name of the environment variable storing the application environment.
  abstract EnvironmentVariable: string with get

  /// Value indicating whether the application uses a 32-bit process.
  member val Is32Bit = not Environment.Is64BitOperatingSystem with get, set

  /// The application manifest.
  member val Manifest = ApplicationManifest.Default with get, set

  /// The path to the application root directory.
  member val Path = "" with get, set

  /// The program used to run this application.
  abstract Program: string with get
