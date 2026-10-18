namespace Belin.Cli

open System.IO

/// Contains operations for working with service applications.
module ServiceProcess =

  /// The path to the resource directory.
  let private resources: string =
    typeof<Architecture>.Assembly.Location |> Path.GetDirectoryName |> fun name -> Path.Join(name, "../Resources/ServiceProcess")

  /// Returns th absolute path of the `nssm` program according to the specified process architecture.
  let getNssmPath (architecture: Architecture) =
    Path.Join(resources, $"nssm.{(string architecture).ToLowerInvariant()}.exe") |> Path.GetFullPath
