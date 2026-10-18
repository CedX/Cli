namespace Belin.Cli

open Microsoft.VisualStudio.TestTools.UnitTesting
open System.IO

/// Tests the features of the `ServiceProcess` module.
[<TestClass>]
type ServiceProcessTests() =

  [<TestMethod>]
  [<DataRow(Architecture.X64)>]
  [<DataRow(Architecture.X86)>]
  member _.GetNssmPath(architecture: Architecture) =
    let path = ServiceProcess.getNssmPath architecture
    path |> Should.endWith ($"/Resources/ServiceProcess/nssm.{(string architecture).ToLowerInvariant()}.exe".Replace('/', Path.DirectorySeparatorChar))
    path |> File.Exists |> Should.beTrue
