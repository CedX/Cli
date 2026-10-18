namespace Belin.Cli

open System
open System.IO
open System.Runtime.Versioning

/// Contains operations interacting with system processes.
module Diagnostics =

  /// Returns the architecture of the specified Windows executable.
  [<SupportedOSPlatform("windows")>]
  let getExecutableArchitecture (path: string) =
    use stream = File.OpenRead path
    use reader = new BinaryReader(stream)

    stream.Seek(0x3C, SeekOrigin.Begin) |> ignore<int64>
    stream.Seek(reader.ReadInt32() + 4 |> int64, SeekOrigin.Begin) |> ignore<int64>
    match reader.ReadUInt16() with
    | 0x014Cus -> Architecture.X86
    | 0x8664us -> Architecture.X64
    | value -> raise (NotSupportedException (sprintf "Unsupported machine type: 0x%X" value))

  /// Runs the specified executable with the `--version` argument.
  /// Returns the standard output of the underlying process.
  let getExecutableVersion (path: string) =
    // let startInfo = Proces
    ()
