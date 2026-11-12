#!/usr/bin/env pwsh
$ErrorActionPreference = "Stop"
$PSNativeCommandUseErrorActionPreference = $true
dotnet "$PSScriptRoot/Binaries/Belin.Cli.dll" @args
