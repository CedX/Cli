namespace Belin.Cli.Commands

open Belin.Cli
open Belin.Cli.Options
open System
open System.CommandLine
open System.IO
open System.Net.Http
open System.Net.Http.Json
open System.Threading.Tasks

/// Represents a PowerShell release.
type PowerShellRelease = {

  /// The Git tag of the current stable release.
  StableReleaseTag: string
}

/// Downloads and installs the latest PowerShell release.
type InstallPowerShell(httpClient: HttpClient) as this =
  inherit Command("powershell", "Install the latest PowerShell release.")

  /// The path to the destination directory.
  let destinationOption =
    DestinationDirectory(if OperatingSystem.IsWindows() then @"C:\Program Files\PowerShell" else "/opt/powershell")

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
      let executable = Path.Join(destinationPath, if OperatingSystem.IsWindows() then "pwsh.exe" else "pwsh")

      Console.WriteLine $@"Extracting the archive into the directory ""{destinationPath}""..."
      match archive.Extension with
      | ".zip" -> Compression.ExpandZipArchive(archive.FullName, destinationPath)
      | _ -> Compression.ExpandTarArchive(archive.FullName, destinationPath)

      if not (OperatingSystem.IsWindows()) then File.makeExecutable executable
      executable |> Diagnostics.getExecutableVersion |> Console.WriteLine
      0

  /// Downloads the PowerShell release corresponding to the specified version.
  /// Returns the path to the downloaded archive.
  member private _.DownloadArchive(version: Version): Task<FileInfo> = task {
    let platform, architecture, extension =
      if OperatingSystem.IsMacOS() then "osx", "arm64", "tar.gz"
      elif OperatingSystem.IsWindows() then "win", "x64", "zip"
      else "linux", "x64", "tar.gz"

    let file = $"powershell-{version}-{platform}-{architecture}.{extension}"
    Console.WriteLine $@"Downloading the file ""{file}""..."

    let! bytes = httpClient.GetByteArrayAsync $"https://github.com/PowerShell/PowerShell/releases/download/v{version}/{file}"
    let tempFile = $"{Path.GetTempFileName()}{extension}"
    do! File.WriteAllBytesAsync(tempFile, bytes)
    return FileInfo tempFile
  }

  /// Fetches the latest version number of PowerShell releases.
  member private _.FetchLatestVersion(): Task<Version> = task {
    Console.WriteLine "Fetching the list of PowerShell releases..."
    let metadataUrl = "https://raw.githubusercontent.com/PowerShell/PowerShell/master/tools/metadata.json"
    match! httpClient.GetFromJsonAsync<PowerShellRelease> metadataUrl with
    | null -> return failwith "Unable to fetch the list of PowerShell releases."
    | release -> return Version.Parse release.StableReleaseTag[1..]
  }
