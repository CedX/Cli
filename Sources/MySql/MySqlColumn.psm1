using namespace System.ComponentModel.DataAnnotations.Schema

<#
.SYNOPSIS
	Provides the metadata of a table column.
#>
[Table("COLUMNS")]
class MySqlColumn {

	<#
	.SYNOPSIS
		The column name.
	#>
	[Column("COLUMN_NAME")]
	[string] $Name = ""

	<#
	.SYNOPSIS
		The column position.
	#>
	[Column("ORDINAL_POSITION")]
	[int] $Position

	<#
	.SYNOPSIS
		The schema containing this column.
	#>
	[Column("TABLE_SCHEMA")]
	[string] $Schema = ""

	<#
	.SYNOPSIS
		The table containing this column.
	#>
	[Column("TABLE_NAME")]
	[string] $Table = ""
}
