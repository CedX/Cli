<#
.SYNOPSIS
	Tests the features of the `Get-NssmPath` cmdlet.
#>
Describe "Get-NssmPath" {
	BeforeAll { . "$PSScriptRoot/../Sources/ServiceProcess.ps1" }

	It "should return the path of the ""nssm"" program according to the given process architecture" -ForEach "x64", "x86" {
		$path = $_ | Get-NssmPath
		Should-BeLikeString ("*/Resources/ServiceProcess/nssm.$_.exe" -replace "/", ($IsWindows ? "\" : "/")) $path -CaseSensitive
		Should-BeTrue (Test-Path $path)
	}
}
