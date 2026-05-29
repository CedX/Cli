<#
.SYNOPSIS
	Downloads and installs the Microsoft Build of OpenJDK.
.OUTPUTS
	The output from the `java --version` command.
#>
function Install-Jdk {
	[CmdletBinding()]
	[OutputType([string])]
	param (
		# The path to the output directory.
		[Parameter(Position = 0)]
		[ValidateScript({ Test-Path $_ -IsValid }, ErrorMessage = "The specified output path is invalid.")]
		[string] $DestinationPath = $IsWindows ? "C:\Program Files\OpenJDK" : "/opt/openjdk",

		# The major version of the Java development kit.
		[ValidateSet(11, 17, 21, 25)]
		[int] $Version = 25
	)

	if (-not (Test-IsPrivilegedProcess $DestinationPath)) {
		throw [UnauthorizedAccessException] "You must run this command in an elevated prompt."
	}

	$platform, $extension = switch ($true) {
		$IsMacOS { "macOS", "tar.gz"; break }
		$IsWindows { "windows", "zip"; break }
		default { "linux", "tar.gz" }
	}

	$file = "microsoft-jdk-$Version-$platform-x64.$extension"
	"Downloading file ""$file""..."
	$outputFile = New-TemporaryFile
	Invoke-WebRequest "https://aka.ms/download-jdk/$file" -OutFile $outputFile

	"Extracting file ""$file"" into directory ""$DestinationPath""..."
	if ($extension -eq "zip") { Expand-ZipArchive $outputFile -DestinationPath $DestinationPath -Skip 1 }
	else { Expand-TarArchive $outputFile -DestinationPath $DestinationPath -Skip 1 }

	$executable = $IsWindows ? "java.exe" : "java"
	& "$DestinationPath/bin/$executable" --version
}

<#
.SYNOPSIS
	Downloads and installs the latest Node.js release.
.OUTPUTS
	The output from the `node --version` command.
#>
function Install-Node {
	[CmdletBinding()]
	[OutputType([string])]
	param (
		# The path to the output directory.
		[Parameter(Position = 0)]
		[ValidateScript({ Test-Path $_ -IsValid }, ErrorMessage = "The specified output path is invalid.")]
		[string] $DestinationPath = $IsWindows ? "C:\Program Files\Node.js" : "/usr/local",

		# The path to the NSSM configuration file.
		[ValidateScript({ Test-Path $_ -PathType Leaf }, ErrorMessage = "The specified NSSM configuration file does not exist.")]
		[string] $Config = ""
	)

	$nssmConfig = $Config ? (Import-PowerShellDataFile $Config) : @{}
	$services = $nssmConfig.Keys.Where{ $_ -eq [Environment]::MachineName }.ForEach{ $nssmConfig.$_ }

	if (-not (Test-IsPrivilegedProcess ($services ? "" : $DestinationPath))) {
		throw [UnauthorizedAccessException] "You must run this command in an elevated prompt."
	}

	$platform, $extension = switch ($true) {
		$IsMacOS { "darwin", "tar.gz"; break }
		$IsWindows { "win", "zip"; break }
		default { "linux", "tar.xz" }
	}

	"Fetching the list of Node.js releases..."
	$response = Invoke-RestMethod https://nodejs.org/dist/index.json
	$version = [semver] $response[0].version.Substring(1)

	$file = "node-v$version-$platform-x64.$extension"
	"Downloading file ""$file""..."
	$outputFile = New-TemporaryFile
	Invoke-WebRequest "https://nodejs.org/dist/v$version/$file" -OutFile $outputFile

	if ($services) {
		"Stopping the NSSM services..."
		$services | Stop-Service
	}

	"Extracting file ""$file"" into directory ""$DestinationPath""..."
	if ($extension -eq "zip") { Expand-ZipArchive $outputFile -DestinationPath $DestinationPath -Skip 1 }
	else { Expand-TarArchive $outputFile -DestinationPath $DestinationPath -Skip 1 }

	if (-not $IsWindows) {
		"CHANGELOG.md", "LICENSE", "README.md" | ForEach-Object { Join-Path $DestinationPath $_ } | Remove-Item -ErrorAction Ignore
	}

	if ($services) {
		"Starting the NSSM services..."
		$services | Start-Service
	}

	$executable = $IsWindows ? "node.exe" : "bin/node"
	& "$DestinationPath/$executable" --version
}

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
