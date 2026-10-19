
<#
.SYNOPSIS
	Gets the list of all collations.
.OUTPUTS
	The list of all collations.
#>
function Select-MySqlCollation {
	[CmdletBinding()]
	[OutputType([string])]
	param (
		# The connection to the data source.
		[Parameter(Mandatory, Position = 1)]
		[IDbConnection] $Connection
	)

	$records = Invoke-SqlQuery $Connection -Command "SHOW COLLATION"
	$records.ForEach{ $_.Collation }
}

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

<#
.SYNOPSIS
	Gets the list of all storage engines.
.OUTPUTS
	The list of all storage engines.
#>
function Select-MySqlEngine {
	[CmdletBinding()]
	[OutputType([string])]
	param (
		# The connection to the data source.
		[Parameter(Mandatory, Position = 1)]
		[IDbConnection] $Connection
	)

	$records = Invoke-SqlQuery $Connection -Command "SHOW ENGINES"
	$records.ForEach{ $_.Engine }
}

<#
.SYNOPSIS
	Gets the list of schemas hosted by a database server.
.OUTPUTS
	The schemas hosted by the database server.
#>
function Select-MySqlSchema {
	[CmdletBinding()]
	[OutputType([MySqlSchema])]
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
