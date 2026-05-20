using namespace System.Data

<#
.SYNOPSIS
	Gets the list of all collations.
.OUTPUTS
	The list of all collations.
#>
function Get-MySqlCollation {
	[CmdletBinding()]
	[OutputType([string])]
	param (
		# The connection to the data source.
		[Parameter(Mandatory, Position = 0)]
		[IDbConnection] $Connection
	)

	$records = Invoke-SqlQuery $Connection -Command "SHOW COLLATION"
	$records.ForEach{ $_.Collation }
}
