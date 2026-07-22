using namespace System.ComponentModel.DataAnnotations.Schema

<#
.SYNOPSIS
	Provides the metadata of a database table.
#>
[Table("TABLES")]
class MySqlTable {

	<#
	.SYNOPSIS
		The default collation.
	#>
	[Column("TABLE_COLLATION")]
	[string] $Collation = ""

	<#
	.SYNOPSIS
		The storage engine.
	#>
	[Column("ENGINE")]
	[string] $Engine = [MySqlTableEngine]::None

	<#
	.SYNOPSIS
		The table name.
	#>
	[Column("TABLE_NAME")]
	[string] $Name = ""

	<#
	.SYNOPSIS
		The schema containing this table.
	#>
	[Column("TABLE_SCHEMA")]
	[string] $Schema = ""

	<#
	.SYNOPSIS
		The table type.
	#>
	[Column("TABLE_TYPE")]
	[string] $Type = [MySqlTableType]::BaseTable

	<#
	.SYNOPSIS
		Gets the fully qualified name.
	.OUTPUTS
		The fully qualified name.
	#>
	[string] QualifiedName() {
		return $this.GetQualifiedName($false)
	}

	<#
	.SYNOPSIS
		Gets the fully qualified name.
	.PARAMETER Escape
		Value indicating whether to escape the SQL identifiers.
	.OUTPUTS
		The fully qualified name.
	#>
	[string] GetQualifiedName([bool] $Escape) {
		$scriptBlock = $Escape ? { param ([string] $value) "``$value``" } : { param ([string] $value) $value }
		return "$(& $scriptBlock $this.Schema).$(& $scriptBlock $this.Name)"
	}
}

<#
.SYNOPSIS
	Defines the storage engine of a table.
#>
class MySqlTableEngine {

	<#
	.SYNOPSIS
		The table does not use any storage engine.
	#>
	static [string] $None = ""

	<#
	.SYNOPSIS
		The storage engine is Aria.
	#>
	static [string] $Aria = "Aria"

	<#
	.SYNOPSIS
		The storage engine is InnoDB.
	#>
	static [string] $InnoDB = "InnoDB"

	<#
	.SYNOPSIS
		The storage engine is MyISAM.
	#>
	static [string] $MyISAM = "MyISAM"
}

<#
.SYNOPSIS
	Defines the type of a table.
#>
class MySqlTableType {

	<#
	.SYNOPSIS
		A base table.
	#>
	static [string] $BaseTable = "BASE TABLE"

	<#
	.SYNOPSIS
		A view.
	#>
	static [string] $View = "VIEW"
}
