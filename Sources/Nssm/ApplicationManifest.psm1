using namespace System.IO

<#
.SYNOPSIS
	Provides information about a web application.
#>
class ApplicationManifest {

	<#
	.SYNOPSIS
		The application description.
	#>
	[ValidateNotNull()]
	[string] $Description = ""

	<#
	.SYNOPSIS
		The application environment.
	#>
	[ValidateNotNull()]
	[string] $Environment = ""

	<#
	.SYNOPSIS
		The application identifier.
	#>
	[ValidateNotNull()]
	[string] $Id = ""

	<#
	.SYNOPSIS
		The application name.
	#>
	[ValidateNotNull()]
	[string] $Name = ""

	<#
	.SYNOPSIS
		Reads the application manifest located at the specified path.
	.PARAMETER Path
		The path to the manifest file.
	.OUTPUTS
		The application manifest corresponding to the specified file.
	#>
	static [ApplicationManifest] Read([string] $Path) {
		$manifest = switch (Split-Path $Path -Extension) {
			".config" { ([xml] (Get-Content $Path -Raw)).Configuration; break }
			".json" { Get-Content $Path -Raw | ConvertFrom-Json; break }
			".psd1" { Import-PowerShellDataFile $Path; break }
			".xml" { ([xml] (Get-Content $Path -Raw)).Configuration; break }
			default { throw [NotSupportedException] "The ""$_"" file format is not supported." }
		}

		return [ApplicationManifest]@{
			Description = $manifest.Description ?? ""
			Environment = $manifest.Environment ?? ""
			Id = $manifest.Id
			Name = $manifest.Name
		}
	}
}
