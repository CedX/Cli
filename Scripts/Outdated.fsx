#load "Tools.fsx"
open Tools

writeOutput "Checking for outdated dependencies..."
startProcess "dotnet" "package list --outdated"
