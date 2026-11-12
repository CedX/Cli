namespace Belin.Cli.Install

open Belin.Cli
open System
open System.CommandLine
open System.IO
open System.Net.Http
open System.Net.Http.Json
open System.Text.RegularExpressions
open System.Threading.Tasks

/// Represents a Node.js release.
type NodeRelease = {

  /// The version number.
  Version: string
}

/// Downloads and installs the latest Node.js release.
type InstallNodeCommand(httpClient: HttpClient) as this =
  inherit Command("node", "Install the latest Node.js release.")

  /// The path to the destination directory.
  let destinationOption =
    DestinationOption(if OperatingSystem.IsWindows() then @"C:\Program Files\Node.js" else "/usr/local")

  do
    this.Options.Add destinationOption
    this.SetAction this.Invoke

  /// Invokes this command.
  member _.Invoke(parseResult: ParseResult): int =
    let destinationPath = (parseResult.GetRequiredValue destinationOption).FullName
    if not (Security.canWriteTo destinationPath) then
      Console.Error.WriteLine "You must run this command in an elevated prompt."
      1
    else
      let version = this.FetchLatestVersion().GetAwaiter().GetResult()
      let archive = this.DownloadArchive(version).GetAwaiter().GetResult()
      let executable = Path.Join(destinationPath, if OperatingSystem.IsWindows() then "node.exe" else "bin/node")

      Console.WriteLine $@"Extracting the archive into the directory ""{destinationPath}""..."
      match archive.Extension with
      | ".zip" -> Compression.ExpandZipArchive(archive.FullName, destinationPath, skip = 1)
      | _ -> Compression.ExpandTarArchive(archive.FullName, destinationPath, skip = 1)

      if not (OperatingSystem.IsWindows()) then
        ["CHANGELOG.md"; "LICENSE"; "README.md"] |> List.iter (fun file -> Path.Join(destinationPath, file) |> File.Delete)
        File.makeExecutable executable

      executable |> Diagnostics.getExecutableVersion |> Console.WriteLine
      0

  /// Downloads the Node.js release corresponding to the specified version.
  /// Returns the path to the downloaded archive.
  member private _.DownloadArchive(version: Version): Task<FileInfo> = task {
    let platform, architecture, extension =
      if OperatingSystem.IsMacOS() then "darwin", "arm64", "tar.gz"
      elif OperatingSystem.IsWindows() then "win", "x64", "zip"
      else "linux", "x64", "tar.gz"

    let file = $"node-v{version}-{platform}-{architecture}.{extension}"
    Console.WriteLine $@"Downloading the file ""{file}""..."

    let! bytes = httpClient.GetByteArrayAsync $"https://nodejs.org/dist/v{version}/{file}"
    let tempFile = $"{Path.GetTempFileName()}{extension}"
    do! File.WriteAllBytesAsync(tempFile, bytes)
    return FileInfo tempFile
  }

  /// Fetches the latest version number of Node.js releases.
  member private _.FetchLatestVersion(): Task<Version> = task {
    Console.WriteLine "Fetching the list of Node.js releases..."
    let versionPattern = Regex @"^v\d+(\.\d+){2}$"
    match! httpClient.GetFromJsonAsync<NodeRelease list> "https://nodejs.org/dist/index.json" with
    | null -> return failwith "Unable to fetch the list of Node.js releases."
    | releases ->
      return releases
      |> List.find (fun release -> versionPattern.IsMatch release.Version)
      |> fun release -> Version.Parse release.Version[1..]
  }
