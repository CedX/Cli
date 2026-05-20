using namespace System.Data

<#
.SYNOPSIS
	Gets the list of all storage engines.
.OUTPUTS
	The list of all storage engines.
#>
function Get-MySqlEngine {
	[CmdletBinding()]
	[OutputType([string])]
	param (
		# The connection to the data source.
		[Parameter(Mandatory, Position = 0)]
		[IDbConnection] $Connection
	)

	$records = Invoke-SqlQuery $Connection -Command "SHOW ENGINES"
	$records.ForEach{ $_.Engine }
}
