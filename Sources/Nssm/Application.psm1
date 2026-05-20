using namespace System.IO
using module ./ApplicationManifest.psm1

<#
.SYNOPSIS
	Represents a web application.
#>
class Application {

	<#
	.SYNOPSIS
		Value indicating whether the application uses a 32-bit process.
	#>
	[bool] $Is32Bit = -not [Environment]::Is64BitOperatingSystem

	<#
	.SYNOPSIS
		The application manifest.
	#>
	[ValidateNotNull()]
	[ApplicationManifest] $Manifest = [ApplicationManifest]::new()

	<#
	.SYNOPSIS
		The path to the application root directory.
	#>
	[ValidateNotNullOrEmpty()]
	[string] $Path

	<#
	.SYNOPSIS
		Creates a new application.
	#>
	Application([string] $Path) {
		$this.Path = [Path]::TrimEndingDirectorySeparator((Resolve-Path $Path))

		foreach ($folder in "Sources/Server", "Sources") {
			$files = ("config", "json", "psd1", "xml").ForEach{ Join-Path $this.Path -ChildPath $folder "appsettings.$_" }.Where({ Test-Path $_ -PathType Leaf }, "First")
			if ($files.Count) { $this.Manifest = [ApplicationManifest]::Read($files[0]); break }
		}

		if (-not $this.Manifest.Id) {
			throw [EntryPointNotFoundException] "Unable to locate the application manifest."
		}
	}

	<#
	.SYNOPSIS
		Gets the entry point of this application.
	.OUTPUTS
		The entry point of this application.
	#>
	[string] EntryPoint() {
		throw [NotImplementedException]::new()
	}

	<#
	.SYNOPSIS
		Gets the name of the environment variable storing the application environment.
	.OUTPUTS
		The name of the environment variable storing the application environment.
	#>
	[string] EnvironmentVariable() {
		throw [NotImplementedException]::new()
	}

	<#
	.SYNOPSIS
		Gets the program used to run this application.
	.OUTPUTS
		The program used to run this application.
	#>
	[string] Program() {
		throw [NotImplementedException]::new()
	}
}
