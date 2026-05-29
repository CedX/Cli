using namespace System.Data
using module ./MySqlSchema.psm1

<#
.SYNOPSIS
	Gets the list of schemas hosted by a database server.
.OUTPUTS
	The schemas hosted by the database server.
#>
function Get-MySqlSchema {
	[CmdletBinding()]
	[OutputType([MySqlSchema])]
	param (
		# The connection to the data source.
		[Parameter(Mandatory, Position = 0)]
		[IDbConnection] $Connection
	)

	Invoke-SqlQuery $Connection -As ([MySqlSchema]) -Command "
		SELECT *
		FROM information_schema.SCHEMATA
		WHERE SCHEMA_NAME NOT IN ('information_schema', 'mysql', 'performance_schema', 'sys')
		ORDER BY SCHEMA_NAME"
}
