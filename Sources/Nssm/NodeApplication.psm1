using namespace System.Diagnostics.CodeAnalysis
using module ./Application.psm1

<#
.SYNOPSIS
	Represents a Node.js application.
#>
class NodeApplication: Application {

	<#
	.SYNOPSIS
		The path of the application entry point.
	#>
	[ValidateNotNull()]
	hidden [string] $EntryPath = ""

	<#
	.SYNOPSIS
		Creates a new application.
	.PARAMETER Path
		The path to the application root directory.
	#>
	NodeApplication([string] $Path): base($Path) {
		if ($file = Get-Item "$($this.Path)/package.json" -ErrorAction Ignore) {
			$package = Get-Content $file.FullName -Raw | ConvertFrom-Json -AsHashtable
			if (-not $this.Manifest.Description) { $this.Manifest.Description = $package.description ?? "" }
			if (-not $this.Manifest.Name) { $this.Manifest.Name = $package.name }

			$keys = $package.bin?.Keys
			if ($keys) { $this.EntryPath = Join-Path $this.Path $package.bin[$keys[0]] -Resolve -ErrorAction Ignore }
		}
	}

	<#
	.SYNOPSIS
		Gets the entry point of this application.
	.OUTPUTS
		The entry point of this application.
	#>
	[string] EntryPoint() {
		if ($this.EntryPath) { return $this.EntryPath }
		throw [EntryPointNotFoundException]::new("Unable to resolve the application entry point.")
	}

	<#
	.SYNOPSIS
		Gets the name of the environment variable storing the application environment.
	.OUTPUTS
		The name of the environment variable storing the application environment.
	#>
	[string] EnvironmentVariable() {
		return "NODE_ENV"
	}

	<#
	.SYNOPSIS
		Gets the program used to run this application.
	.OUTPUTS
		The program used to run this application.
	#>
	[string] Program() {
		return [OperatingSystem]::IsWindows() ? "node.exe" : "node"
	}
}
