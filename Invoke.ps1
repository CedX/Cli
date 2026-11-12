#!/usr/bin/env pwsh
param (
	# The script to invoke.
	[Parameter(Position = 1)]
	[ArgumentCompleter({
		param ([string] $commandName, [string] $parameterName, [string] $wordToComplete)
		(Get-Item "$PSScriptRoot/Scripts/$wordToComplete*.fsx").BaseName
	})]
	[ValidateScript(
		{ Test-Path "$PSScriptRoot/Scripts/$_.fsx" -PathType Leaf },
		ErrorMessage = "The specified script does not exist."
	)]
	[string] $Script = "Default",

	# The script arguments.
	[Parameter(ValueFromRemainingArguments)]
	[string[]] $Arguments = @()
)

$ErrorActionPreference = "Stop"
$PSNativeCommandUseErrorActionPreference = $true
dotnet fsi --checknulls "$PSScriptRoot/Scripts/$Script.fsx" @Arguments
