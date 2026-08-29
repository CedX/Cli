using namespace System.Diagnostics.CodeAnalysis
using module ./Architecture.psm1
using module ./ServiceProcess/DotNetApplication.psm1
using module ./ServiceProcess/NodeApplication.psm1
using module ./ServiceProcess/PowerShellApplication.psm1

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
		Join-Path $PSScriptRoot "../Resources/ServiceProcess/nssm.$Architecture.exe" -Resolve
	}
}

<#
.SYNOPSIS
	Registers a Windows service based on [NSSM](https://nssm.cc).
.INPUTS
	The path to the root directory of the web application.
.OUTPUTS
	The log messages.
#>
function New-NssmService {
	[CmdletBinding()]
	[OutputType([string])]
	[SuppressMessage("PSUseShouldProcessForStateChangingFunctions", "")]
	param (
		# The path to the root directory of the web application.
		[Parameter(Position = 1, ValueFromPipeline)]
		[ValidateScript({ Test-Path $_ -PathType Container }, ErrorMessage = "The specified directory does not exist.")]
		[string] $Path = $PWD,

		# The account used by the service as the logon account.
		[Credential()]
		[pscredential] $Credential,

		# Value indicating whether to start the service after its registration.
		[switch] $Start
	)

	begin {
		if (-not $IsWindows) { throw [PlatformNotSupportedException]::new("This command only supports the Windows platform.") }
		if (-not (Test-IsPrivilegedProcess)) { throw [UnauthorizedAccessException]::new("You must run this command in an elevated prompt.") }
	}

	process {
		$application = switch ($true) {
			((Test-Path "$Path/Sources/Server/*.cs") -or (Test-Path "$Path/Sources/*.cs")) { [DotNetApplication]::new($Path); break }
			((Test-Path "$Path/Sources/Server/*.ps1") -or (Test-Path "$Path/Sources/*.ps1")) { [PowerShellApplication]::new($Path); break }
			((Test-Path "$Path/Sources/Server/*.ts") -or (Test-Path "$Path/Sources/*.ts")) { [NodeApplication]::new($Path); break }
			default { throw [NotSupportedException]::new("The application type could not be determined.") }
		}

		if (Get-Service $application.Manifest.Id -ErrorAction Ignore) {
			throw [InvalidOperationException]::new("The service ""$($application.Manifest.Id)"" already exists.")
		}

		$properties = [ordered]@{
			AppDirectory = $application.Path
			AppEnvironmentExtra = "$($application.EnvironmentVariable())=$($application.Manifest.Environment)"
			AppNoConsole = "1"
			AppStderr = Join-Path $application.Path Temp/Error.log
			AppStdout = Join-Path $application.Path Temp/Output.log
			Description = $application.Manifest.Description
			DisplayName = $application.Manifest.Name
			Start = "SERVICE_AUTO_START"
		}

		$programPath = (Get-Command $application.Program()).Path
		if ($IsWindows -and [Environment]::Is64BitOperatingSystem -and $application.Is32Bit) {
			$programPath = $programPath -replace "\\Program Files\\", "\Program Files (x86)\"
		}

		$nssm = (Get-Command nssm -ErrorAction Ignore) ?? (Get-NssmPath)
		& $nssm install $application.Manifest.Id $programPath $application.EntryPoint() | Out-Null
		foreach ($key in $properties.Keys) { & $nssm set $application.Manifest.Id $key $properties.$key | Out-Null }

		if ($Credential) {
			$password = $Credential.Password.Length ? $Credential.GetNetworkCredential().Password : ""
			& $nssm set $application.Manifest.Id ObjectName $Credential.UserName $password | Out-Null
		}

		if ($Start) { Start-Service $application.Manifest.Id }
		$created = $Start ? "started" : "created"
		"The service ""$($application.Manifest.Id)"" has been successfully $created."
	}
}

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
		[Parameter(Position = 1, ValueFromPipeline)]
		[ValidateScript({ Test-Path $_ -PathType Container }, ErrorMessage = "The specified directory does not exist.")]
		[string] $Path = $PWD
	)

	begin {
		if (-not $IsWindows) { throw [PlatformNotSupportedException]::new("This command only supports the Windows platform.") }
		if (-not (Test-IsPrivilegedProcess)) { throw [UnauthorizedAccessException]::new("You must run this command in an elevated prompt.") }
	}

	process {
		$application = switch ($true) {
			((Test-Path "$Path/Sources/Server/*.cs") -or (Test-Path "$Path/Sources/*.cs")) { [DotNetApplication]::new($Path); break }
			((Test-Path "$Path/Sources/Server/*.ps1") -or (Test-Path "$Path/Sources/*.ps1")) { [PowerShellApplication]::new($Path); break }
			((Test-Path "$Path/Sources/Server/*.ts") -or (Test-Path "$Path/Sources/*.ts")) { [NodeApplication]::new($Path); break }
			default { throw [NotSupportedException]::new("The application type could not be determined.") }
		}

		if (-not (Get-Service $application.Manifest.Id -ErrorAction Ignore)) {
			throw [InvalidOperationException]::new("The service ""$($application.Manifest.Id)"" does not exist.")
		}
		else {
			Stop-Service $application.Manifest.Id
			Remove-Service $application.Manifest.Id
			"The service ""$($application.Manifest.Id)"" has been successfully removed."
		}
	}
}
