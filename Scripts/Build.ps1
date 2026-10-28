& "$PSScriptRoot/Assets.ps1"

Write-Information "Building the solution..."
dotnet build --configuration ($Release ? "Release" : "Debug")
