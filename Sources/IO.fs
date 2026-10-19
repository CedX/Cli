namespace Belin.Cli

open System.Diagnostics
open System.IO
open System.IO.Compression
open System.Management.Automation
open System.Runtime.Versioning

/// Contains operations for decompressing archives.
type Compression =

  /// Extracts the specified TAR archive into a given directory.
  static member ExpandTarArchive(path: string, destinationPath: string, ?skip: int) =
    Directory.CreateDirectory destinationPath |> ignore<DirectoryInfo>

    let startInfo = ProcessStartInfo("tar", CreateNoWindow = true, arguments = [
      "--directory"; destinationPath
      "--extract"
      "--file"; path
      "--strip-components"; string (defaultArg skip 0)
    ])

    let message = $"{startInfo.FileName} {startInfo.Arguments}"
    match Process.Start startInfo with
    | null -> raise (ApplicationFailedException message)
    | startedProcess ->
      startedProcess.WaitForExit()
      if startedProcess.ExitCode <> 0 then raise (ApplicationFailedException message)

  /// Extracts the specified ZIP archive into a given directory.
  static member ExpandZipArchive(path: string, destinationPath: string, ?skip: int) =
    let components = defaultArg skip 0
    if components <= 0 then
      ZipFile.ExtractToDirectory(path, destinationPath, overwriteFiles = true)
    else
      use zipArchive = ZipFile.OpenRead path
      for entry in zipArchive.Entries do
        let mutable entryPath = entry.FullName.Split '/' |> Array.skip components |> String.concat "/"
        if entryPath.Length = 0 then entryPath <- "/"

        let fullPath = Path.Join(destinationPath, entryPath)
        if fullPath[fullPath.Length - 1] = '/' then Directory.CreateDirectory fullPath |> ignore<DirectoryInfo>
        else entry.ExtractToFile(fullPath, overwrite = true)

/// Contains operations for reading and writing to files.
module File =

  /// The Unix file mode of an executable file.
  let private executePermission =
    UnixFileMode.UserRead ||| UnixFileMode.UserWrite ||| UnixFileMode.UserExecute
    ||| UnixFileMode.GroupRead ||| UnixFileMode.GroupExecute
    ||| UnixFileMode.OtherRead ||| UnixFileMode.OtherExecute

  /// Sets the execute permission on the specified file.
  [<UnsupportedOSPlatform("windows")>]
  let makeExecutable (path: string) = File.SetUnixFileMode(path, executePermission)
