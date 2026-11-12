#load "Tools.fsx"
open System
open System.IO
open Tools

let isRelease = IsRelease // Prevents a parsing bug in FSharpLint.
if isRelease then startProcess "dotnet" "fsi Scripts/Default.fsx --release"
else writeError @"The ""--release"" switch must be set!"

writeOutput "Tagging the repository..."
startProcess "git" $"tag v{Version}"
startProcess "git" $"push origin v{Version}"

writeOutput "Publishing the package..."
startProcess "dotnet" "pack --no-build --output Temp/NuGet"

let apiKey = Environment.GetEnvironmentVariable "NUGET_API_KEY"
Directory.EnumerateFiles("Temp/NuGet", "*.nupkg")
|> Seq.map (fun file -> $@"nuget push ""{file}"" --api-key {apiKey} --source NuGet")
|> Seq.iter (fun arguments -> startProcess "dotnet" arguments)
