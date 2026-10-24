namespace Belin.Cli

open Microsoft.VisualStudio.TestTools.UnitTesting
open System
open System.IO
open System.Text.RegularExpressions

/// Tests the features of the `Diagnostics` module.
[<TestClass>]
type DiagnosticsTests() =

  [<TestMethod>]
  [<DataRow("nssm.x64.exe", Architecture.X64)>]
  [<DataRow("nssm.x86.exe", Architecture.X86)>]
  member _.GetExecutableArchitecture(executable: string, expected: Architecture) =
    Path.Join(AppContext.BaseDirectory, $"../Resources/ServiceProcess/{executable}") |> Diagnostics.getExecutableArchitecture |> Should.be expected

  [<TestMethod>]
  member _.GetExecutableVersion() =
    "dotnet" |> Diagnostics.getExecutableVersion |> Should.matchRegex (Regex @"^\d+(\.\d+){2}$")
