using namespace System.Data
using module ./MySqlColumn.psm1
using module ./MySqlTable.psm1

<#
.SYNOPSIS
	Gets the list of columns contained in the specified table.
.INPUTS
	The database table.
.OUTPUTS
	The columns contained in the specified table.
#>
function Select-MySqlColumn {
	[CmdletBinding()]
	[OutputType([MySqlColumn])]
	param (
		# The connection to the data source.
		[Parameter(Mandatory, Position = 1)]
		[IDbConnection] $Connection,

		# The database table.
		[Parameter(Mandatory, Position = 2, ValueFromPipeline)]
		[MySqlTable] $Table
	)

	process {
		$sql = "
			SELECT *
			FROM information_schema.COLUMNS
			WHERE TABLE_SCHEMA = @Schema AND TABLE_NAME = @Name
			ORDER BY ORDINAL_POSITION"

		Invoke-SqlQuery $Connection -As ([MySqlColumn]) -Command $sql -Parameters @{
			Name = $Table.Name
			Schema = $Table.Schema
		}
	}
}
