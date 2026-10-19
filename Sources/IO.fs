namespace Belin.Cli

open System.IO
open System.Runtime.Versioning

/// Contains operations for reading and writing to files and data streams.
module IO =

  /// The Unix file mode of an executable file.
  let private executePermission =
    UnixFileMode.UserRead ||| UnixFileMode.UserWrite ||| UnixFileMode.UserExecute
    ||| UnixFileMode.GroupRead ||| UnixFileMode.GroupExecute
    ||| UnixFileMode.OtherRead ||| UnixFileMode.OtherExecute

  /// Sets the execute permission on the specified file.
  [<UnsupportedOSPlatform("windows")>]
  let makeFileExecutable (path: string) = File.SetUnixFileMode(path, executePermission)
