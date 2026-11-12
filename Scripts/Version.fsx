#load "Tools.fsx"
open System.IO
open System.Text.RegularExpressions
open Tools

writeOutput "Updating the version number in the sources..."
let pattern = Regex @"<Version>\d+(\.\d+){2}</Version>"
for file in Directory.EnumerateFiles(".", "*.fsproj", SearchOption.AllDirectories) do
  File.ReadAllText file
  |> fun contents -> pattern.Replace(contents, $"<Version>{Version}</Version>")
  |> fun contents -> File.WriteAllText(file, contents)
