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
}
