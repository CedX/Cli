<#
.SYNOPSIS
	Tests the features of the `Test-IsExcludedFile` cmdlet.
#>
Describe "Test-IsExcludedFile" {
	BeforeAll { . "$PSScriptRoot/../Sources/Text.ps1" }

	It "should return `$false if the file path does not contain any excluded folder" -ForEach @(
		"C:\Users\Cedric\.gitconfig"
		"/usr/local/bin/pwsh"
	) {
		$_ | Test-IsExcludedFile | Should-BeFalse
	}

	It "should return `$true if the file path contains an excluded folder" -ForEach @(
		"C:\Projects\Cli\.git\config"
		"/var/www/ps_modules/Pester/Pester.ps1"
	) {
		$_ | Test-IsExcludedFile | Should-BeTrue
	}
}
