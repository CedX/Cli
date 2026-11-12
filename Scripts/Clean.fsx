#load "Tools.fsx"
open System.IO
open Tools

writeOutput "Deleting all generated files..."
Directory.EnumerateDirectories(".", "obj", SearchOption.AllDirectories) |> Seq.iter removeItem
removeItem "Binaries"
clearItem "Temp"
