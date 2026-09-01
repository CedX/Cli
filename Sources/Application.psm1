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
			default { throw [NotSupportedException]::new("The ""$_"" file format is not supported.") }
		}

		return [ApplicationManifest]@{
			Description = $manifest.Description ?? ""
			Environment = $manifest.Environment ?? ""
			Id = $manifest.Id
			Name = $manifest.Name
		}
	}
}

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
			throw [EntryPointNotFoundException]::new("Unable to locate the application manifest.")
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
		if ($file = Get-Item "$($this.Path)/Sources/Server/*.csproj" -ErrorAction Ignore || Get-Item "$($this.Path)/Sources/*.csproj" -ErrorAction Ignore) {
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
