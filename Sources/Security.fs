namespace Belin.Cli

open System
open System.IO

/// Contains security-related operations.
module Security =

  /// Checks whether the current process is privileged.
  /// A path to a directory can be specified to verify if the process has sufficient access permissions.
  let isPrivilegedProcess (directory: string option): bool =
    match directory with
    | None -> Environment.IsPrivilegedProcess
    | Some path ->
      if Environment.IsPrivilegedProcess then true
      else
        let homeDirectory = Environment.SpecialFolder.Personal |> Environment.GetFolderPath |> DirectoryInfo
        let targetDirectory = DirectoryInfo path
        targetDirectory.Root.Name <> homeDirectory.Root.Name || targetDirectory.FullName.StartsWith homeDirectory.FullName
