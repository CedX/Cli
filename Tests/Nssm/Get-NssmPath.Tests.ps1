<#
.SYNOPSIS
	Tests the features of the `Get-NssmPath` cmdlet.
#>
Describe "Get-NssmPath" {
	BeforeAll { . "$PSScriptRoot/../../Sources/Nssm/Get-NssmPath.ps1" }

	It "should return the path of the ""nssm"" program according to the given process architecture" -ForEach "x64", "x86" {
		$path = $_ | Get-NssmPath
		$path | Should -BeLikeExactly ("*/Resources/Nssm/nssm.$_.exe" -replace "/", ($IsWindows ? "\" : "/"))
		$path | Should -Exist
	}
}
