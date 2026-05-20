using module ./Cmdlets.psm1

if ($Release) { & "$PSScriptRoot/Default.ps1" }
else {
	"The ""-Release"" switch must be set!"
	exit 1
}

"Publishing the module..."
$version = (Import-PowerShellDataFile Cli.psd1).ModuleVersion
New-GitTag "v$version"
Publish-PSGalleryModule
