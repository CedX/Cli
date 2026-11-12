namespace Belin.Cli.Install

open Belin.Cli
open System
open System.CommandLine
open System.IO
open System.Net.Http
open System.Threading.Tasks

/// Downloads and installs the latest OpenJDK release.
type JdkCommand(httpClient: HttpClient) as this =
  inherit Command("jdk", "Install the latest OpenJDK release.")

  /// The path to the destination directory.
  let destinationOption =
    DestinationOption(if OperatingSystem.IsWindows() then @"C:\Program Files\Node.js" else "/usr/local")

  /// The major version of the Java development kit.
  let versionOption =
    let option = Option<int>("--version", [| "-v" |], HelpName = "version")
    option.DefaultValueFactory <- fun _ -> 25
    option.Description <- "The major version of the Java development kit."
    option.AcceptOnlyFromAmong("11", "17", "21", "25")

  do
    this.Options.Add destinationOption
    this.Options.Add versionOption
    this.SetAction this.Invoke

  /// Invokes this command.
  member _.Invoke(parseResult: ParseResult): int =
    let destinationPath = (parseResult.GetRequiredValue destinationOption).FullName
    let version = parseResult.GetRequiredValue versionOption

    if not (Security.canWriteTo destinationPath) then
      Console.Error.WriteLine "You must run this command in an elevated prompt."
      1
    else
      let archive = this.DownloadArchive(version).GetAwaiter().GetResult()
      let executable = Path.Join(destinationPath, if OperatingSystem.IsWindows() then @"bin\java.exe" else "bin/java")

      Console.WriteLine $@"Extracting the archive into the directory ""{destinationPath}""..."
      match archive.Extension with
      | ".zip" -> Compression.ExpandZipArchive(archive.FullName, destinationPath, skip = 1)
      | _ -> Compression.ExpandTarArchive(archive.FullName, destinationPath, skip = 1)

      if not (OperatingSystem.IsWindows()) then File.makeExecutable executable
      executable |> Diagnostics.getExecutableVersion |> Console.WriteLine
      0

  /// Downloads the OpenJDK release corresponding to the specified version.
  /// Returns the path to the downloaded archive.
  member private _.DownloadArchive(version: int): Task<FileInfo> = task {
    let platform, architecture, extension =
      if OperatingSystem.IsMacOS() then "macOS", "aarch64", "tar.gz"
      elif OperatingSystem.IsWindows() then "windows", "x64", "zip"
      else "linux", "x64", "tar.gz"

    let file = $"microsoft-jdk-{version}-{platform}-{architecture}.{extension}"
    Console.WriteLine $@"Downloading the file ""{file}""..."

    let! bytes = httpClient.GetByteArrayAsync $"https://aka.ms/download-jdk/{file}"
    let tempFile = $"{Path.GetTempFileName()}{extension}"
    do! File.WriteAllBytesAsync(tempFile, bytes)
    return FileInfo tempFile
  }
