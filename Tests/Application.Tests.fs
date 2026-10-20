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
