using namespace System.IO

<#
.SYNOPSIS
	Checks if the specified file should be excluded from the processing.
.INPUTS
	The file to be checked.
.OUTPUTS
	`$true` if the specified file should be excluded from the processing, otherwise `$false`.
#>
function Test-IsExcludedFile {
	[CmdletBinding()]
	[OutputType([bool])]
	param (
		# The file to be checked.
		[Parameter(Mandatory, Position = 0, ValueFromPipeline)]
		[FileInfo] $File,

		# The list of folders to exclude from the processing.
		[ValidateNotNull()]
		[string[]] $Exclude = @(".git", "node_modules", "ps_modules", "vendor")
	)

	process {
		$directory = $File.Directory
		while ($directory) {
			if ($directory.Name -in $Exclude) { return $true }
			$directory = $directory.Parent
		}

		$false
	}
}
