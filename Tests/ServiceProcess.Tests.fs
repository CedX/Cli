namespace Belin.Cli

open Microsoft.VisualStudio.TestTools.UnitTesting
open System
open System.IO

/// Tests the features of the `ApplicationManifest` class.
[<TestClass>]
type ApplicationManifestTests() =

  [<TestMethod>]
  member _.Read() =
    for file in Path.Join(AppContext.BaseDirectory, "../Tests/Fixtures") |> Directory.EnumerateFiles do
      let manifest = ApplicationManifest.Read file
      Should.beEmpty manifest.Description
      Should.beEmpty manifest.Environment
      manifest.Id |> Should.be "MyApp"
      manifest.Name |> Should.be "My Application 1.0"

/// Tests the features of the `Nssm` module.
[<TestClass>]
type NssmTests() =

  [<TestMethod>]
  [<DataRow(Architecture.X64, "nssm.x64.exe")>]
  [<DataRow(Architecture.X86, "nssm.x86.exe")>]
  member _.GetPath(architecture: Architecture, executable: string) =
    let path = Nssm.getPath architecture
    path |> Should.endWith ($"/Resources/ServiceProcess/{executable}".Replace('/', Path.DirectorySeparatorChar))
    path |> File.Exists |> Should.beTrue
