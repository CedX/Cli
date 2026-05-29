using module ../Sources/Nssm/ApplicationManifest.psm1

<#
.SYNOPSIS
	Tests the features of the `ApplicationManifest` class.
#>
Describe "ApplicationManifest" {
	Context "Read" {
		It "should support the JSON manifests" -ForEach "json", "psd1", "xml" {
			$manifest = [ApplicationManifest]::Read("$PSScriptRoot/Fixtures/Manifest.$_")
			$manifest.Description | Should -BeNullOrEmpty
			$manifest.Environment | Should -BeNullOrEmpty
			$manifest.Id | Should -BeExactly "MyApp"
			$manifest.Name | Should -BeExactly "My Application 1.0"
		}
	}
}

<#
.SYNOPSIS
	Tests the features of the `Get-NssmPath` cmdlet.
#>
Describe "Get-NssmPath" {
	BeforeAll { . "$PSScriptRoot/../Sources/Nssm.ps1" }

	It "should return the path of the ""nssm"" program according to the given process architecture" -ForEach "x64", "x86" {
		$path = $_ | Get-NssmPath
		$path | Should -BeLikeExactly ("*/Resources/Nssm/nssm.$_.exe" -replace "/", ($IsWindows ? "\" : "/"))
		$path | Should -Exist
	}
}
