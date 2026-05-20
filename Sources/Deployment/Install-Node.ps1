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
