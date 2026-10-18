namespace Belin.Cli

open System.IO
open System.Management.Automation
open System.Text.Json
open System.Text.Json.Serialization
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

  /// Reads the application manifest located at the specified path.
  static member Read(path: string) =
    let manifest =
      match (nonNull (Path.GetExtension path)).ToLowerInvariant() with
      | ".config" | ".xml" ->
        use xmlReader = XmlReader.Create path
        XmlSerializer(typeof<ApplicationManifest>).Deserialize xmlReader :?> ApplicationManifest | null |> defaultIfNull ApplicationManifest.Default
      | ".json" ->
        JsonSerializer.Deserialize<ApplicationManifest>(File.ReadAllText path, JsonSerializerOptions.Web) |> defaultIfNull ApplicationManifest.Default
      | _ -> ApplicationManifest.Default


    ()
