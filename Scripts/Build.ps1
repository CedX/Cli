using module ./Cmdlets.psm1
& "$PSScriptRoot/Assets.ps1"

"Building the solution..."
Build-DotNetSolution ($Release ? "Release" : "Debug")
