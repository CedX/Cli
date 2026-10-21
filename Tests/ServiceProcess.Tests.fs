namespace Belin.Cli

open Microsoft.VisualStudio.TestTools.UnitTesting
open System
open System.IO

/// Tests the features of the `ApplicationManifest` class.
[<TestClass>]
type ApplicationManifestTests() =

  [<TestMethod>]
  member _.Read() =
    for file in Path.Join(AppContext.BaseDirectory, "../Tests/Fixtures") |> Directory.GetFiles do
      let manifest = ApplicationManifest.Read file
      Should.beEmpty manifest.Description
      Should.beEmpty manifest.Environment
      manifest.Id |> Should.be "MyApp"
      manifest.Name |> Should.be "My Application 1.0"

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
