namespace Belin.Cli

open Microsoft.VisualStudio.TestTools.UnitTesting
open System.IO

/// Tests the features of the `ServiceProcess` module.
[<TestClass>]
type ServiceProcessTests() =

  [<TestMethod>]
  [<DataRow(Architecture.X64, "nssm.x64.exe")>]
  [<DataRow(Architecture.X86, "nssm.x86.exe")>]
  member _.GetNssmPath(architecture: Architecture, executable: string) =
    let path = ServiceProcess.getNssmPath architecture
    path |> Should.endWith ($"/Resources/ServiceProcess/{executable}".Replace('/', Path.DirectorySeparatorChar))
    path |> File.Exists |> Should.beTrue
