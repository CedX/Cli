<#
.SYNOPSIS
	Tests the features of the `Get-ExecutableArchitecture` cmdlet.
#>
Describe "Get-ExecutableArchitecture" {
	BeforeAll { . "$PSScriptRoot/../../Sources/Diagnostics/Get-ExecutableArchitecture.ps1" }

	It "should return the architecture of the given executable" -ForEach "x64", "x86" {
		"$PSScriptRoot/../../Resources/Nssm/nssm.$_.exe" | Get-ExecutableArchitecture | Should -Be $_
	}
}
