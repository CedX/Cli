using namespace System.Data
using module ./MySqlSchema.psm1
using module ./MySqlTable.psm1

<#
.SYNOPSIS
	Gets the list of tables contained in the specified schema.
.INPUTS
	The database schema.
.OUTPUTS
	The tables contained in the specified schema.
#>
function Select-MySqlTable {
	[CmdletBinding()]
	[OutputType([Belin.Cli.Data.MySqlTable])]
	param (
		# The connection to the data source.
		[Parameter(Mandatory, Position = 1)]
		[IDbConnection] $Connection,

		# The database schema.
		[Parameter(Mandatory, Position = 2, ValueFromPipeline)]
		[MySqlSchema] $Schema
	)

	process {
		$sql = "
			SELECT *
			FROM information_schema.TABLES
			WHERE TABLE_SCHEMA = @Name AND TABLE_TYPE = @Type
			ORDER BY TABLE_NAME"

		Invoke-SqlQuery $Connection -As ([MySqlTable]) -Command $sql -Parameters @{
			Name = $Schema.Name
			Type = [MySqlTableType]::BaseTable
		}
	}
}
