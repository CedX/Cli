namespace Belin.Cli

open System.Diagnostics
open System.IO
open System.IO.Compression
open System.Linq
open System.Management.Automation

/// Contains operations for decompressing archives.
module Compression =

  /// Extracts the specified TAR archive into a given directory.
  let expandTarArchive (path: string) (destinationPath: string) (skip: int option) =
    Directory.CreateDirectory destinationPath |> ignore

    let startInfo = ProcessStartInfo("tar", CreateNoWindow = true, arguments = [
      "--directory"; destinationPath;
      "--extract";
      "--file"; path;
      "--strip-components"; string (defaultArg skip 0)
    ])

    match Process.Start startInfo with
    | null -> raise (ApplicationFailedException startInfo.FileName)
    | proc -> proc.WaitForExit(); if proc.ExitCode <> 0 then raise (ApplicationFailedException startInfo.FileName)

  /// Extracts the specified ZIP archive into a given directory.
  let expandZipArchive (path: string) (destinationPath: string) (skip: int option) =
    use zipArchive = ZipFile.OpenRead path

    let components = defaultArg skip 0
    if components <= 0 then zipArchive.ExtractToDirectory(destinationPath, overwriteFiles = true)
    else
      for entry in zipArchive.Entries do
        let mutable entryPath = entry.FullName.Split('/').Skip components |> String.concat "/"
        if entryPath.Length = 0 then entryPath <- "/"

        let fullPath = Path.Join(destinationPath, entryPath)
        if fullPath[-1] = '/' then Directory.CreateDirectory fullPath |> ignore
        else entry.ExtractToFile(fullPath, overwrite = true)
