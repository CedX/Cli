using namespace Belin.Cli.Data
using namespace System.Data

<#
.SYNOPSIS
	Gets the list of schemas hosted by a database server.
.OUTPUTS
	The schemas hosted by the database server.
#>
function Select-MySqlSchema {
	[CmdletBinding()]
	[OutputType([Belin.Cli.Data.MySqlSchema])]
	param (
		# The connection to the data source.
		[Parameter(Mandatory, Position = 1)]
		[IDbConnection] $Connection
	)

	Invoke-SqlQuery $Connection -As ([MySqlSchema]) -Command "
		SELECT *
		FROM information_schema.SCHEMATA
		WHERE SCHEMA_NAME NOT IN ('information_schema', 'mysql', 'performance_schema', 'sys')
		ORDER BY SCHEMA_NAME"
}
