<#
.SYNOPSIS
	Represents a .NET application.
#>
class DotNetApplication: Application {

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
	DotNetApplication([string] $Path): base($Path) {
		if ($file = Get-Item "$($this.Path)/Sources/Server/*.*proj" -ErrorAction Ignore || Get-Item "$($this.Path)/Sources/*.*proj" -ErrorAction Ignore) {
			$entryPoint = @{ AssemblyName = ""; Platforms = ""; OutDir = "" }

			foreach ($propertyGroup in ([xml] (Get-Content $file.FullName -Raw)).Project.PropertyGroup) {
				if (-not $this.Manifest.Description) { $this.Manifest.Description = $propertyGroup.Description ?? "" }
				if (-not $this.Manifest.Name) { $this.Manifest.Name = $propertyGroup.Product ?? "" }
				if (-not $entryPoint.AssemblyName) { $entryPoint.AssemblyName = $propertyGroup.AssemblyName }
				if (-not $entryPoint.Platforms) { $entryPoint.Platforms = $propertyGroup.Platforms ?? "" }
				if ((-not $entryPoint.OutDir) -and $propertyGroup.OutDir) { $entryPoint.OutDir = Join-Path $file.DirectoryName $propertyGroup.OutDir }
			}

			if (-not $entryPoint.AssemblyName) { $entryPoint.AssemblyName = $file.BaseName }
			if (-not $entryPoint.OutDir) { $entryPoint.OutDir = Join-Path $this.Path bin }

			$this.EntryPath = Join-Path $entryPoint.OutDir "$($entryPoint.AssemblyName).dll" -Resolve -ErrorAction Ignore
			if ($entryPoint.Platforms) { $this.Is32Bit = ($entryPoint.Platforms -split ";") -contains "x86" }
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
		return "DOTNET_ENVIRONMENT"
	}

	<#
	.SYNOPSIS
		Gets the program used to run this application.
	.OUTPUTS
		The program used to run this application.
	#>
	[string] Program() {
		return [OperatingSystem]::IsWindows() ? "dotnet.exe" : "dotnet"
	}
}

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
		throw [EntryPointNotFoundException]::new("Unable to resolve the application entry point.")
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
