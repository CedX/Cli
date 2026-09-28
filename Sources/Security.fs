namespace Belin.Cli

open System
open System.IO

/// Contains security-related operations.
module Security =

  /// TODO Checks whether the current process is privileged.
  /// A path to a directory can be specified to verify if the process has sufficient permissions. TODO according to the folder???
  let isPrivilegedProcess (path: string option): bool =
    match path with
    | None -> Environment.IsPrivilegedProcess
    | Some value ->
      if Environment.IsPrivilegedProcess then true
      else
        let homeDirectory = DirectoryInfo (Environment.GetFolderPath Environment.SpecialFolder.Personal)
        let targetDirectory = DirectoryInfo value
        targetDirectory.Root.Name <> homeDirectory.Root.Name || targetDirectory.FullName.StartsWith homeDirectory.FullName
