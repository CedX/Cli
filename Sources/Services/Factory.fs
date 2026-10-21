namespace Belin.Cli.Services

open Belin.Cli
open System
open System.IO

/// Contains operations for creating service applications.
module ServiceFactory =

  /// Creates a new service from the specified application directory.
  let newService (path: string) =
    let sources =
      Directory.EnumerateFiles(Path.Join(path, "Sources/Server"))
      |> Seq.append (Directory.EnumerateFiles(Path.Join(path, "Sources")))
      |> Seq.toArray

    let mutable index = 0
    let mutable service: Service option = None
    while service.IsNone && index < sources.Length do
      let file = sources[index]
      index <- index + 1

      service <-
        match (Path.GetExtension file |> defaultIfNull "").ToLowerInvariant() with
        | ".cs" | ".fs" -> Some (DotNetService file)
        | ".js" | ".ts" -> Some (NodeService file)
        | ".psm1" | ".ps1" -> Some (PowerShellService file)
        | _ -> None

    match service with
    | Some object -> object
    | None -> raise (NotSupportedException "The service type could not be determined.")
