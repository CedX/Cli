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
