namespace Belin.Cli.Install

open Belin.Cli
open Microsoft.Win32
open System
open System.Collections.Generic
open System.CommandLine
open System.IO
open System.Net.Http
open System.Net.Http.Json
open System.ServiceProcess
open System.Threading.Tasks

/// Represents a PHP release.
type PhpRelease = {

  /// The version number.
  Version: string
}

/// Downloads and installs the latest PHP release.
type PhpCommand(httpClient: HttpClient) as this =
  inherit Command("php", "Install the latest PHP release.")

  /// The path to the destination directory.
  let destinationOption =
    DestinationOption(if OperatingSystem.IsWindows() then @"C:\Program Files\PHP" else "/usr/local")

  /// Value indicating whether to register the PHP interpreter with the event log.
  let registerEventSourceOption =
    let option = Option<bool> "--register-event-source"
    option.Description <- "Value indicating whether to register the PHP interpreter with the event log."
    option

  do
    this.Options.Add destinationOption
    this.Options.Add registerEventSourceOption
    this.SetAction this.Invoke

  /// Invokes this command.
  member _.Invoke(parseResult: ParseResult): int =
    let destinationPath = (parseResult.GetRequiredValue destinationOption).FullName
    let registerEventSource = parseResult.GetRequiredValue registerEventSourceOption

    if not (OperatingSystem.IsWindows()) then
      eprintfn "This command only supports the Windows platform."
      1
    elif not (Security.canWriteTo destinationPath) then
      eprintfn "You must run this command in an elevated prompt."
      2
    else
      let version = this.FetchLatestVersion().GetAwaiter().GetResult()
      let archive = this.DownloadArchive(version).GetAwaiter().GetResult()
      let executable = Path.Join(destinationPath, if OperatingSystem.IsWindows() then "php.exe" else "bin/php")

      use serviceController = new ServiceController "W3SVC"
      if Environment.IsPrivilegedProcess then ServiceProcess.stopService serviceController

      printn $@"Extracting the archive into the directory ""{destinationPath}""..."
      Compression.ExpandZipArchive(archive.FullName, destinationPath)

      if Environment.IsPrivilegedProcess then
        ServiceProcess.startService serviceController
        if registerEventSource then this.RegisterEventLog(version, destinationPath)

      if not (OperatingSystem.IsWindows()) then File.makeExecutable executable
      executable |> Diagnostics.getExecutableVersion |> printn
      0

  /// Downloads the PHP release corresponding to the specified version.
  /// Returns the path to the downloaded archive.
  member private _.DownloadArchive(version: Version): Task<FileInfo> = task {
    let vs = if version >= Version "8.4.0" then "vs17" else "vs16"
    let architecture = (string Architecture.Current).ToLowerInvariant()
    let file = $"php-{version}-nts-Win32-{vs}-{architecture}.zip"
    printn $@"Downloading the file ""{file}""..."

    let! bytes = httpClient.GetByteArrayAsync $"https://downloads.php.net/~windows/releases/archives/{file}"
    let tempFile = $"{Path.GetTempFileName()}.zip"
    do! File.WriteAllBytesAsync(tempFile, bytes)
    return FileInfo tempFile
  }

  /// Fetches the latest version number of PHP releases.
  member private _.FetchLatestVersion(): Task<Version> = task {
    printn "Fetching the list of PHP releases..."
    match! httpClient.GetFromJsonAsync<Dictionary<string, PhpRelease>> "https://www.php.net/releases/?json" with
    | null -> return failwith "Unable to fetch the list of PHP releases."
    | releases -> return releases |> Seq.head |> fun release -> Version.Parse release.Value.Version
  }

  /// Registers the PHP interpreter with the event log.
  member private _.RegisterEventLog(phpVersion: Version, phpDirectory: string) =
    printn "Registering the PHP interpreter with the event log..."
    let key = $@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\EventLog\Application\PHP-{phpVersion}"
    Registry.SetValue(key, "EventMessageFile", Path.GetFullPath($"php{phpVersion.Major}.dll", phpDirectory), RegistryValueKind.String)
    Registry.SetValue(key, "TypesSupported", 7, RegistryValueKind.DWord)
