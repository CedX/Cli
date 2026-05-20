<#
.SYNOPSIS
	Downloads and installs the latest PHP release.
.OUTPUTS
	The output from the `php --version` command.
#>
function Install-Php {
	[CmdletBinding()]
	[OutputType([string])]
	param (
		# The path to the output directory.
		[Parameter(Position = 0)]
		[ValidateScript({ Test-Path $_ -IsValid }, ErrorMessage = "The specified output path is invalid.")]
		[string] $DestinationPath = "C:\Program Files\PHP",

		# Value indicating whether to register the PHP interpreter with the event log.
		[switch] $RegisterEventSource
	)

	if (-not $IsWindows) { throw [PlatformNotSupportedException] "This command only supports the Windows platform." }
	if (-not (Test-IsPrivilegedProcess $DestinationPath)) { throw [UnauthorizedAccessException] "You must run this command in an elevated prompt." }

	"Fetching the list of PHP releases..."
	$response = Invoke-RestMethod "https://www.php.net/releases/?json"
	$property = ($response | Get-Member -MemberType NoteProperty | Sort-Object Name -Bottom 1).Name
	$version = [semver] $response.$property.version

	$file = "php-$version-nts-Win32-$($version -ge [semver] "8.4.0" ? "vs17" : "vs16")-x64.zip"
	"Downloading file ""$file""..."
	$outputFile = New-TemporaryFile
	Invoke-WebRequest "https://downloads.php.net/~windows/releases/archives/$file" -OutFile $outputFile

	if ([Environment]::IsPrivilegedProcess) {
		"Stopping the IIS web server..."
		Stop-Service W3SVC
	}

	"Extracting file ""$file"" into directory ""$DestinationPath""..."
	Expand-Archive $outputFile $DestinationPath -Force

	if ([Environment]::IsPrivilegedProcess) {
		"Starting the IIS web server..."
		Start-Service W3SVC
	}

	if ($RegisterEventSource) {
		"Registering the PHP interpreter with the event log..."
		$key = "HKLM:\SYSTEM\CurrentControlSet\Services\EventLog\Application\PHP-$version"
		New-Item $key -Force | Out-Null
		New-ItemProperty $key EventMessageFile -Force -PropertyType String -Value (Join-Path $DestinationPath "php$($version.Major).dll" -Resolve) | Out-Null
		New-ItemProperty $key TypesSupported -Force -PropertyType DWord -Value 7 | Out-Null
	}

	& "$DestinationPath/php.exe" --version
}
