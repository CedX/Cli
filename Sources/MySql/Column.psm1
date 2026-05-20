using namespace System.ComponentModel.DataAnnotations.Schema

<#
.SYNOPSIS
	Provides the metadata of a table column.
#>
[Table("COLUMNS")]
class Column {

	<#
	.SYNOPSIS
		The column name.
	#>
	[ColumnAttribute("COLUMN_NAME")]
	[string] $Name = ""

	<#
	.SYNOPSIS
		The column position.
	#>
	[ColumnAttribute("ORDINAL_POSITION")]
	[int] $Position

	<#
	.SYNOPSIS
		The schema containing this column.
	#>
	[ColumnAttribute("TABLE_SCHEMA")]
	[string] $Schema = ""

	<#
	.SYNOPSIS
		The table containing this column.
	#>
	[ColumnAttribute("TABLE_NAME")]
	[string] $Table = ""
}
