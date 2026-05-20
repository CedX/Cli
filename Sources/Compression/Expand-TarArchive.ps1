<#
.SYNOPSIS
	Extracts the specified TAR archive into a given directory.
#>
function Expand-TarArchive {
	[CmdletBinding()]
	[OutputType([void])]
	param (
		# The path to the input TAR archive.
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

	New-Item $DestinationPath -Force -ItemType Directory | Out-Null
	tar --directory $DestinationPath --extract --file $Path --strip-components $Skip
}
