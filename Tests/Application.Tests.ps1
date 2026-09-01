using module ../Sources/Application.psm1

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
