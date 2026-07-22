using namespace Belin.Cli.Diagnostics

<#
.SYNOPSIS
	Returns the path of the `nssm` program according to the specified process architecture.
.INPUTS
	The process architecture.
.OUTPUTS
	The absolute path of the `nssm` program.
#>
function Get-NssmPath {
	[CmdletBinding()]
	[OutputType([string])]
	param (
		# The process architecture.
		[Parameter(Position = 1, ValueFromPipeline)]
		[Architecture] $Architecture = [Environment]::Is64BitOperatingSystem ? [Architecture]::x64 : [Architecture]::x86
	)

	process {
		Join-Path $PSScriptRoot "../../Resources/ServiceProcess/nssm.$Architecture.exe" -Resolve
	}
}
