namespace Belin.Cli

open System
open System.Diagnostics
open System.IO
open System.Management.Automation
open System.Runtime.Versioning

/// An enumeration of instruction set architectures.
type Architecture =
  | X64 = 1
  | X86 = 2

/// COntains operations for working with architectures.
module Architecture =

  /// The current architecture.
  let Current = if Environment.Is64BitOperatingSystem then Architecture.X64 else Architecture.X86

/// Contains operations for interacting with system processes.
module Diagnostics =

  /// Returns the architecture of the specified Windows executable.
  [<SupportedOSPlatform("windows")>]
  let getExecutableArchitecture (path: string): Architecture =
    use stream = File.OpenRead path
    use reader = new BinaryReader(stream)

    stream.Seek(0x3C, SeekOrigin.Begin) |> ignore<int64>
    stream.Seek(reader.ReadInt32() + 4 |> int64, SeekOrigin.Begin) |> ignore<int64>
    match reader.ReadUInt16() with
    | 0x014Cus -> Architecture.X86
    | 0x8664us -> Architecture.X64
    | value -> raise (NotSupportedException (sprintf "Unsupported machine type: 0x%X" value))

  /// Runs the specified command with the `--version` argument, and returns its standard output.
  let getExecutableVersion (command: string): string =
    let startInfo = ProcessStartInfo(command, ["--version"], CreateNoWindow = true, RedirectStandardOutput = true)
    let message = $"{startInfo.FileName} {startInfo.Arguments}"

    match Process.Start startInfo with
    | null -> raise (ApplicationFailedException message)
    | startedProcess ->
      let stdout = startedProcess.StandardOutput.ReadToEnd().Trim()
      startedProcess.WaitForExit()
      if startedProcess.ExitCode <> 0 then raise (ApplicationFailedException message)
      stdout
