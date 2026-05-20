using module ../Diagnostics/Architecture.psm1

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
		[Parameter(Position = 0, ValueFromPipeline)]
		[Architecture] $Architecture = [Environment]::Is64BitOperatingSystem ? [Architecture]::x64 : [Architecture]::x86
	)

	process {
		Join-Path $PSScriptRoot "../../Resources/Nssm/nssm.$Architecture.exe" -Resolve
	}
}
