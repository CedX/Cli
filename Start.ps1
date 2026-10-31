#!/usr/bin/env pwsh
param (
	# The command to invoke.
	[Parameter(Mandatory, Position = 1)]
	[ArgumentCompleter({
		param ([string] $commandName, [string] $parameterName, [string] $wordToComplete)
		$module = Import-PowerShellDataFile "$PSScriptRoot/Cli.psd1"
		$cmdlets = $module.CmdletsToExport + $module.FunctionsToExport
		$cmdlets -like "$wordToComplete*"
	})]
	[ValidateScript({
		$module = Import-PowerShellDataFile "$PSScriptRoot/Cli.psd1"
		$cmdlets = $module.CmdletsToExport + $module.FunctionsToExport
		$cmdlets -contains $_
	}, ErrorMessage = "The specified command does not exist.")]
	[string] $Command,

	# The command arguments.
	[Parameter(ValueFromRemainingArguments)]
	[string[]] $Arguments = @()
)

$scriptBlock = {
	param ([string] $scriptRoot, [string] $command, [string[]] $arguments)

	$ErrorActionPreference = "Stop"
	$InformationPreference = "Continue"
	$PSNativeCommandUseErrorActionPreference = $true

	Import-Module "$scriptRoot/Cli.psd1"
	$argumentList = $arguments | ForEach-Object { $_ -like "* *" ? "'$_'" : $_ }
	Invoke-Expression "$command $($argumentList -join " ")"
}

pwsh -Command $scriptBlock -args $PSScriptRoot, $Command, $Arguments
