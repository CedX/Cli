using namespace System.IO.Compression

<#
.SYNOPSIS
	Extracts the specified ZIP archive into a given directory.
#>
function Expand-ZipArchive {
	[CmdletBinding()]
	[OutputType([void])]
	param (
		# The path to the input ZIP archive.
		[Parameter(Mandatory, Position = 0)]
		[ValidateScript({ Test-Path $_ -PathType Leaf }, ErrorMessage = "The specified input file does not exist.")]
		[string] $Path,

		# The path to the output directory.
		[Parameter(Mandatory, Position = 1)]
		[ValidateScript({ Test-Path $_ -IsValid }, ErrorMessage = "The specified output path is invalid.")]
		[string] $DestinationPath,

		# The number of leading directory components to remove from file names on extraction.
		[ValidateRange("NonNegative")]
		[int] $Skip = 0
	)

	if (-not $Skip) { Expand-Archive $Path $DestinationPath -Force }
	else {
		$archive = [ZipFile]::OpenRead($Path)
		foreach ($entry in $archive.Entries) {
			$components = $entry.FullName -split "/"
			$newPath = $components[$Skip..($components.Count - 1)] -join "/"
			if (-not $newPath) { $newPath = "/" }

			$fullPath = Join-Path $DestinationPath $newPath
			if ($newPath[-1] -eq "/") { New-Item $fullPath -Force -ItemType Directory | Out-Null }
			else { [ZipFileExtensions]::ExtractToFile($entry, $fullPath, $true) }
		}

		$archive.Dispose()
	}
}
