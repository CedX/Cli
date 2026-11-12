#load "Tools.fsx"
#load "Assets.fsx"
open Tools

writeOutput "Watching for file changes..."
startProcess "dotnet" $"watch build --configuration {Configuration} --project Sources"
