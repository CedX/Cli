using assembly ../../Binaries/Belin.Cli.dll

<#
.SYNOPSIS
	Tests the features of the `Get-ExecutableArchitecture` cmdlet.
#>
Describe "Get-ExecutableArchitecture" {
	BeforeAll { . "$PSScriptRoot/../../Sources/Diagnostics/Get-ExecutableArchitecture.ps1" }

	It "should return the architecture of the given executable" -ForEach "x64", "x86" {
		Should-Be $_ ("$PSScriptRoot/../../Resources/ServiceProcess/nssm.$_.exe" | Get-ExecutableArchitecture)
	}
}
