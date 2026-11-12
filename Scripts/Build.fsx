#load "Tools.fsx"
#load "Assets.fsx"
open Tools

writeOutput "Building the solution..."
startProcess "dotnet" $"build --configuration {Configuration}"
