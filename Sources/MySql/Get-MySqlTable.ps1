using namespace System.Data
using module ./Schema.psm1
using module ./Table.psm1

<#
.SYNOPSIS
	Gets the list of tables contained in the specified schema.
.OUTPUTS
	The tables contained in the specified schema.
#>
function Get-MySqlTable {
	[CmdletBinding()]
	[OutputType([Table])]
	param (
		# The connection to the data source.
		[Parameter(Mandatory, Position = 0)]
		[IDbConnection] $Connection,

		# The database schema.
		[Parameter(Mandatory, Position = 1)]
		[Schema] $Schema
	)

	$sql = "
		SELECT *
		FROM information_schema.TABLES
		WHERE TABLE_SCHEMA = @Name AND TABLE_TYPE = @Type
		ORDER BY TABLE_NAME"

	Invoke-SqlQuery $Connection -As ([Table]) -Command $sql -Parameters @{
		Name = $Schema.Name
		Type = [TableType]::BaseTable
	}
}
