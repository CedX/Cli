namespace Belin.Cli.Install

open Belin.Cli
open System
open System.CommandLine
open System.IO

/// Installs the latest NSSM release.
type NssmCommand() as this =
  inherit Command("nssm", "Install the latest NSSM release.")

  /// The path to the destination directory.
  let destinationOption =
    DestinationOption(if OperatingSystem.IsWindows() then @"C:\Windows\System32" else "/usr/local/bin")

  do
    this.Options.Add destinationOption
    this.SetAction this.Invoke

  /// Invokes this command.
  member _.Invoke(parseResult: ParseResult): int =
    let destinationPath = (parseResult.GetRequiredValue destinationOption).FullName

    if not (OperatingSystem.IsWindows()) then
      eprintfn "This command only supports the Windows platform."
      1
    elif not (Security.canWriteTo destinationPath) then
      eprintfn "You must run this command in an elevated prompt."
      2
    else
      printn $@"Copying the binary into the directory ""{destinationPath}""..."
      let executable = Path.Join(destinationPath, if OperatingSystem.IsWindows() then "nssm.exe" else "nssm")
      Directory.CreateDirectory destinationPath |> ignore<DirectoryInfo>
      File.Copy(Nssm.getPath Architecture.Current, executable, overwrite = true)

      if not (OperatingSystem.IsWindows()) then File.makeExecutable executable
      executable |> Diagnostics.getExecutableVersion |> printn
      0
