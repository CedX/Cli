using module ./Application.psm1

<#
.SYNOPSIS
	Represents a PowerShell application.
#>
class PowerShellApplication: Application {

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
	PowerShellApplication([string] $Path): base($Path) {
		if ($file = Get-Item "$($this.Path)/*.psd1" -Exclude PSModules.psd1, PSScriptAnalyzerSettings.psd1 -ErrorAction Ignore) {
			$module = Import-PowerShellDataFile $file.FullName
			if (-not $this.Manifest.Description) { $this.Manifest.Description = $module.Description ?? "" }
			if (-not $this.Manifest.Name) { $this.Manifest.Name = $file.BaseName }
			if ($module.RootModule) { $this.EntryPath = Join-Path $this.Path $module.RootModule -Resolve -ErrorAction Ignore }
		}
	}

	<#
	.SYNOPSIS
		Gets the entry point of this application.
	.OUTPUTS
		The entry point of this application.
	#>
	[string] EntryPoint() {
		if ($this.EntryPath) { return "-ExecutionPolicy Bypass -File ""$($this.EntryPath)"" -NoLogo -NoProfile -NonInteractive" }
		throw [EntryPointNotFoundException] "Unable to resolve the application entry point."
	}

	<#
	.SYNOPSIS
		Gets the name of the environment variable storing the application environment.
	.OUTPUTS
		The name of the environment variable storing the application environment.
	#>
	[string] EnvironmentVariable() {
		return "PODE_ENVIRONMENT"
	}

	<#
	.SYNOPSIS
		Gets the program used to run this application.
	.OUTPUTS
		The program used to run this application.
	#>
	[string] Program() {
		return [OperatingSystem]::IsWindows() ? "pwsh.exe" : "pwsh"
	}
}
