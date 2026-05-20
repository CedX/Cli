using namespace System.Diagnostics.CodeAnalysis
using module ./DotNetApplication.psm1
using module ./NodeApplication.psm1
using module ./PowerShellApplication.psm1

<#
.SYNOPSIS
	Unregisters a Windows service based on [NSSM](https://nssm.cc).
.INPUTS
	The path to the root directory of the web application.
.OUTPUTS
	The log messages.
#>
function Remove-NssmService {
	[CmdletBinding()]
	[OutputType([string])]
	[SuppressMessage("PSUseShouldProcessForStateChangingFunctions", "")]
	param (
		# The path to the root directory of the web application.
		[Parameter(Position = 0, ValueFromPipeline)]
		[ValidateScript({ Test-Path $_ -PathType Container }, ErrorMessage = "The specified directory does not exist.")]
		[string] $Path = $PWD
	)

	begin {
		if (-not $IsWindows) { throw [PlatformNotSupportedException] "This command only supports the Windows platform." }
		if (-not (Test-IsPrivilegedProcess)) { throw [UnauthorizedAccessException] "You must run this command in an elevated prompt." }
	}

	process {
		$application = switch ($true) {
			((Test-Path "$Path/Sources/Server/*.cs") -or (Test-Path "$Path/Sources/*.cs")) { [DotNetApplication] $Path; break }
			((Test-Path "$Path/Sources/Server/*.ps1") -or (Test-Path "$Path/Sources/*.ps1")) { [PowerShellApplication] $Path; break }
			((Test-Path "$Path/Sources/Server/*.ts") -or (Test-Path "$Path/Sources/*.ts")) { [NodeApplication] $Path; break }
			default { throw [NotSupportedException] "The application type could not be determined." }
		}

		if (-not (Get-Service $application.Manifest.Id -ErrorAction Ignore)) {
			throw [InvalidOperationException] "The service ""$($application.Manifest.Id)"" does not exist."
		}
		else {
			Stop-Service $application.Manifest.Id
			Remove-Service $application.Manifest.Id
			"The service ""$($application.Manifest.Id)"" has been successfully removed."
		}
	}
}
