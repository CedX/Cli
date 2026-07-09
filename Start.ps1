#!/usr/bin/env pwsh
param (
	# The name of the cmdlet to run.
	[Parameter(Mandatory, Position = 1)]
	[ArgumentCompleter({
		param ([string] $commandName, [string] $parameterName, [string] $wordToComplete)
		$module = Import-PowerShellDataFile "$PSScriptRoot/Cli.psd1"
		($module.CmdletsToExport + $module.FunctionsToExport) -like "$wordToComplete*"
	})]
	[ValidateScript({
		$module = Import-PowerShellDataFile "$PSScriptRoot/Cli.psd1"
		($module.CmdletsToExport + $module.FunctionsToExport) -contains $_
	}, ErrorMessage = "The specified command does not exist.")]
	[string] $Command,

	# The parameters of the cmdlet to run.
	[Parameter(Position = 2, ValueFromRemainingArguments)]
	[string[]] $Parameters = @()
)

$scriptBlock = {
	param ([string] $scriptRoot, [string] $command, [string[]] $parameters)
	$ErrorActionPreference = "Stop"
	$PSNativeCommandUseErrorActionPreference = $true

	Import-Module "$scriptRoot/Cli.psd1"
	$argumentList = $parameters.ForEach{ $_.Contains(" ") ? "'$_'" : $_ }
	Invoke-Expression "$command $($argumentList -join " ")"
}

pwsh -Command $scriptBlock -args $PSScriptRoot, $Command, $Parameters
