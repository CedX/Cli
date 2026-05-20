using namespace System.IO

<#
.SYNOPSIS
	Checks whether the current process is privileged.
.OUTPUTS
	`$true` if the current process is privileged, otherwise `$false`.
#>
function Test-IsPrivilegedProcess {
	[CmdletBinding()]
	[OutputType([bool])]
	param (
		# The path to a directory used to verify if the process has sufficient permissions.
		[Parameter(Position = 0)]
		[ValidateScript({ Test-Path $_ -IsValid }, ErrorMessage = "The specified path is invalid.")]
		[string] $Path
	)

	$isPrivileged = [Environment]::IsPrivilegedProcess
	if ($Path) {
		$homeDirectory = [DirectoryInfo] $HOME
		$targetDirectory = [DirectoryInfo] $Path
		if (($targetDirectory.Root.Name -ne $homeDirectory.Root.Name) -or $targetDirectory.FullName.StartsWith($homeDirectory.FullName)) { $isPrivileged = $true }
	}

	$isPrivileged
}
