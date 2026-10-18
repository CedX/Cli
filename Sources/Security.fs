namespace Belin.Cli

open System
open System.IO

/// Contains security-related operations.
module Security =

  /// Returns a value indicating whether the current process can write to the specified directory.
  let canWriteTo (directory: string): bool =
    if Environment.IsPrivilegedProcess then true
    else
      let homeDirectory = Environment.SpecialFolder.Personal |> Environment.GetFolderPath |> DirectoryInfo
      let targetDirectory = DirectoryInfo directory
      targetDirectory.Root.Name <> homeDirectory.Root.Name || targetDirectory.FullName.StartsWith homeDirectory.FullName
