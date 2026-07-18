using assembly ../Binaries/Belin.Cli.dll
using module ../Sources/Nssm/ApplicationManifest.psm1

<#
.SYNOPSIS
	Tests the features of the `ApplicationManifest` class.
#>
Describe "ApplicationManifest" {
	Context "Read" {
		It "should support the JSON manifests" -ForEach "json", "psd1", "xml" {
			$manifest = [ApplicationManifest]::Read("$PSScriptRoot/Fixtures/Manifest.$_")
			Should-BeEmptyString $manifest.Description
			Should-BeEmptyString $manifest.Environment
			Should-BeString "MyApp" $manifest.Id -CaseSensitive
			Should-BeString "My Application 1.0" $manifest.Name -CaseSensitive
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
		Should-BeLikeString ("*/Resources/Nssm/nssm.$_.exe" -replace "/", ($IsWindows ? "\" : "/")) $path -CaseSensitive
		Should-BeTrue (Test-Path $path)
	}
}
