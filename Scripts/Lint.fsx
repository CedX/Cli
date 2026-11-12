#load "Tools.fsx"
open Tools

writeOutput "Performing the static analysis of source code..."
let fsharplint = "fsharplint lint --lint-config Configuration/FSharpLint.json"
startProcess "dotnet" $"{fsharplint} {Solution.Name}"
startProcess "dotnet" $"{fsharplint} Scripts/*.fsx"
