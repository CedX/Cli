using namespace System.Data
using module ./Column.psm1
using module ./Table.psm1

<#
.SYNOPSIS
	Gets the list of columns contained in the specified table.
.OUTPUTS
	The columns contained in the specified table.
#>
function Get-MySqlColumn {
	[CmdletBinding()]
	[OutputType([Column])]
	param (
		# The connection to the data source.
		[Parameter(Mandatory, Position = 0)]
		[IDbConnection] $Connection,

		# The database table.
		[Parameter(Mandatory, Position = 1)]
		[Table] $Table
	)

	$sql = "
		SELECT *
		FROM information_schema.COLUMNS
		WHERE TABLE_SCHEMA = @Schema AND TABLE_NAME = @Name
		ORDER BY ORDINAL_POSITION"

	Invoke-SqlQuery $Connection -As ([Column]) -Command $sql -Parameters @{
		Name = $Table.Name
		Schema = $Table.Schema
	}
}
