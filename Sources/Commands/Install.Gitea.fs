namespace Belin.Cli.Commands

open Belin.Cli
open Belin.Cli.Options
open System
open System.Collections.Generic
open System.CommandLine
open System.IO
open System.Net.Http
open System.Net.Http.Json
open System.ServiceProcess
open System.Threading.Tasks

/// Represents a Gitea release.
type GiteaRelease = {

  /// The version number.
  Version: string
}

/// Downloads and installs the latest Gitea release.
type InstallGitea(httpClient: HttpClient) as this =
  inherit Command("gitea", "Install the latest Gitea release.")

  /// The path to the destination directory.
  let destinationOption =
    DestinationDirectory(if OperatingSystem.IsWindows() then @"C:\Gitea" else "/opt/gitea")

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
      let binary = this.DownloadBinary(version).GetAwaiter().GetResult()
      let executable = Path.Join(destinationPath, if OperatingSystem.IsWindows() then "gitea.exe" else "gitea")

      use serviceController = new ServiceController "Gitea"
      if Environment.IsPrivilegedProcess then ServiceProcess.stopService serviceController

      Console.WriteLine $@"Moving the binary into the directory ""{destinationPath}""..."
      Directory.CreateDirectory destinationPath |> ignore<DirectoryInfo>
      File.Copy(binary.FullName, executable, overwrite = true)

      if Environment.IsPrivilegedProcess then ServiceProcess.startService serviceController
      if not (OperatingSystem.IsWindows()) then File.makeExecutable executable
      executable |> Diagnostics.getExecutableVersion |> Console.WriteLine
      0

  /// Downloads the Gitea release corresponding to the specified version.
  /// Returns the path to the downloaded binary.
  member private _.DownloadBinary(version: Version): Task<FileInfo> = task {
    let platform, architecture, extension =
      if OperatingSystem.IsMacOS() then "darwin", "arm64", ""
      elif OperatingSystem.IsWindows() then "windows", "amd64", ".exe"
      else "linux", "amd64", ""

    let file = $"gitea-{version}-{platform}-{architecture}{extension}"
    Console.WriteLine $@"Downloading the file ""{file}""..."

    let! bytes = httpClient.GetByteArrayAsync $"https://dl.gitea.com/gitea/{version}/{file}"
    let tempFile = $"{Path.GetTempFileName()}{extension}"
    do! File.WriteAllBytesAsync(tempFile, bytes)
    return FileInfo tempFile
  }

  /// Fetches the latest version number of Gitea releases.
  member private _.FetchLatestVersion(): Task<Version> = task {
    Console.WriteLine "Fetching the list of Gitea releases..."
    match! httpClient.GetFromJsonAsync<Dictionary<string, GiteaRelease>> "https://dl.gitea.com/gitea/version.json" with
    | null -> return failwith "Unable to fetch the list of Gitea releases."
    | releases -> return releases |> Seq.head |> fun release -> Version.Parse release.Value.Version
  }
