namespace Belin.Cli.Services

open Belin.Cli
open System
open System.IO
open System.Management.Automation
open System.Xml
open System.Xml.Serialization

/// Represents a group of project properties.
type DotNetPropertyGroup() =

  /// The assembly name.
  member val AssemblyName = "" with get, set

  /// The project description.
  member val Description = "" with get, set

  /// The relative path of the project assemblies.
  member val OutDir = "" with get, set

  /// The list of target platforms for assembly, separated by semicolons.
  member val Platforms = "" with get, set

  /// The project description.
  member val Product = "" with get, set

/// Represents the contents of a .NET project file.
[<XmlRoot("Project")>]
type DotNetProject() =

  /// The property groups.
  [<XmlElement("PropertyGroup")>]
  member val PropertyGroups: DotNetPropertyGroup array = [||] with get, set

/// Represents a .NET service.
type DotNetService(path: string) as this =
  inherit Service(path)

  /// The path of the service entry point.
  let mutable entryPoint = ""

  do
    let manifestPath =
      Directory.EnumerateFiles(Path.Join(this.Path, "Sources/Server"), "*.*proj")
      |> Seq.append (Directory.EnumerateFiles(Path.Join(this.Path, "Sources"), "*.*proj"))
      |> Seq.exactlyOne

    use xmlReader = XmlReader.Create manifestPath
    let manifest = XmlSerializer(typeof<DotNetProject>).Deserialize xmlReader :?> DotNetProject | null |> defaultIfNull (DotNetProject())

    let mutable project = struct {| AssemblyName = ""; Platforms = ""; OutDir = "" |}
    for propertyGroup in manifest.PropertyGroups do
      if String.IsNullOrWhiteSpace this.Manifest.Description then this.Manifest.Description <- propertyGroup.Description
      if String.IsNullOrWhiteSpace this.Manifest.Name then this.Manifest.Name <- propertyGroup.Product
      if String.IsNullOrWhiteSpace project.AssemblyName then project <- {| project with AssemblyName = propertyGroup.AssemblyName |}
      if String.IsNullOrWhiteSpace project.Platforms then project <- {| project with Platforms = propertyGroup.Platforms |}
      if String.IsNullOrWhiteSpace project.OutDir && not (String.IsNullOrWhiteSpace propertyGroup.OutDir) then
        project <- {| project with OutDir = Path.Join(Path.GetDirectoryName manifestPath, propertyGroup.OutDir) |}

    if String.IsNullOrWhiteSpace project.AssemblyName then
      project <- {| project with AssemblyName = Path.GetFileNameWithoutExtension manifestPath |> defaultIfNull "" |}
    if String.IsNullOrWhiteSpace project.OutDir then
      project <- {| project with OutDir = Path.Join(this.Path, "Binaries") |}

    let assembly = Path.GetFullPath($"{project.AssemblyName}.dll", project.OutDir)
    if File.Exists assembly then entryPoint <- assembly
    if not (String.IsNullOrWhiteSpace project.Platforms) then this.Is32Bit <- project.Platforms.Split ';' |> Array.contains "x86"

  /// The entry point of this service.
  override val EntryPoint =
    if entryPoint.Length > 0 then entryPoint
    else raise (EntryPointNotFoundException "Unable to resolve the service entry point.")

  /// The name of the environment variable storing the service environment.
  override val EnvironmentVariable = "DOTNET_ENVIRONMENT"

  /// The program used to run this service.
  override val Program = if Platform.IsWindows then "dotnet.exe" else "dotnet"
