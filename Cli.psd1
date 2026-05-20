@{
	ModuleVersion = "5.1.1"
	PowerShellVersion = "7.6"
	RootModule = "Sources/Main.psm1"

	Author = "Cédric Belin <cedx@outlook.com>"
	CompanyName = "Cedric-Belin.fr"
	Copyright = "© Cédric Belin"
	Description = "PowerShell cmdlets for common administrative tasks, such as database management, service management and software installation."
	GUID = "b489d27c-f48e-49b1-b1d4-c99752f2c828"

	AliasesToExport = @()
	CmdletsToExport = @()
	VariablesToExport = @()

	FunctionsToExport = @(
		"Backup-MySqlTable"
		"ConvertTo-Encoding"
		"Install-Jdk"
		"Install-Node"
		"Install-Php"
		"New-NssmService"
		"Optimize-MySqlTable"
		"Remove-NssmService"
		"Restore-MySqlTable"
		"Set-MySqlCharset"
		"Set-MySqlEngine"
	)

	RequiredAssemblies = @(
		"bin/Microsoft.Extensions.DependencyInjection.Abstractions.dll"
		"bin/Microsoft.Extensions.Logging.Abstractions.dll"
		"bin/MySqlConnector.dll"
	)

	RequiredModules = @(
		@{ ModuleName = "Belin.Sql"; ModuleVersion = "1.2.0" }
	)

	PrivateData = @{
		PSData = @{
			LicenseUri = "https://github.com/CedX/Cli/blob/main/License.md"
			ProjectUri = "https://github.com/CedX/Cli"
			ReleaseNotes = "https://github.com/CedX/Cli/releases"
			Tags = "cli", "dotnet", "powershell", "system", "tools"
		}
	}
}
