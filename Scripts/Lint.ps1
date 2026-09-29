using module PSScriptAnalyzer
using module ./Cmdlets.psm1

"Performing the static analysis of source code..."
$PSScriptRoot, "Sources", "Tests" | Invoke-ScriptAnalyzer -Recurse
Invoke-FSharpLint Cli.slnx -Configuration Configuration/FSharpLint.json
Test-ModuleManifest Cli.psd1 | Out-Null
