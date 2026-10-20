namespace Belin.Cli.Services

open Belin.Cli
open System
open System.Management.Automation
open System.Xml.Serialization

/// Represents a .NET service.
type DotNetService(path: string) =
  inherit Service(path)

  /// The path of the service entry point.
  let mutable entryPath = ""

  /// The entry point of this service.
  override val EntryPoint =
    if entryPath.Length > 0 then entryPath
    else raise (EntryPointNotFoundException "Unable to resolve the service entry point.")

  /// The name of the environment variable storing the service environment.
  override val EnvironmentVariable = "DOTNET_ENVIRONMENT"

  /// The program used to run this service.
  override val Program = if Platform.IsWindows then "dotnet.exe" else "dotnet"

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
