namespace Belin.Cli.Install

open System.Diagnostics
open System.IO
open System.IO.Compression

/// Contains operations for decompressing archives.
type Compression =

  /// Extracts the specified TAR archive into a given directory.
  static member ExpandTarArchive(path: string, destinationPath: string, ?skip: int) =
    Directory.CreateDirectory destinationPath |> ignore<DirectoryInfo>

    let arguments = [
      "--directory"; destinationPath
      "--extract"
      "--file"; path
      "--strip-components"; string (defaultArg skip 0)
    ]

    let startInfo = ProcessStartInfo("tar", arguments, CreateNoWindow = true, RedirectStandardError = true)
    match Process.Start startInfo with
    | null -> failwith $"{startInfo.FileName} {startInfo.Arguments}"
    | disposable ->
      use startedProcess = disposable
      let stderr = startedProcess.StandardError.ReadToEnd().Trim()
      startedProcess.WaitForExit()
      if startedProcess.ExitCode <> 0 then failwith stderr

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
