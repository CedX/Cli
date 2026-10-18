namespace Belin.Cli

open Microsoft.VisualStudio.TestTools.UnitTesting
open System.IO

/// Tests the features of the `Text` module.
[<TestClass>]
type TextTests() =

  [<TestMethod>]
  [<DataRow(@"C:\Users\Cedric\.gitconfig", false)>]
  [<DataRow("/usr/local/bin/pwsh", false)>]
  [<DataRow(@"C:\Projects\Cli\.git\config", true)>]
  [<DataRow("/var/www/ps_modules/Pester/Pester.ps1", true)>]
  member _.IsExcludedFile(path: string, isExcluded: bool) =
    let excludedFolders = [| ".git"; "node_modules"; "obj"; "ps_modules"; "vendor" |]
    excludedFolders |> Text.isExcludedFile (FileInfo path) |> Should.be isExcluded
