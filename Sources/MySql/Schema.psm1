using namespace System.ComponentModel.DataAnnotations.Schema

<#
.SYNOPSIS
	Provides the metadata of a database schema.
#>
[Table("SCHEMATA")]
class Schema {

	<#
	.SYNOPSIS
		The default character set.
	#>
	[Column("DEFAULT_CHARACTER_SET_NAME")]
	[string] $Charset = ""

	<#
	.SYNOPSIS
		The default collation.
	#>
	[Column("DEFAULT_COLLATION_NAME")]
	[string] $Collation = ""

	<#
	.SYNOPSIS
		The schema name.
	#>
	[Column("SCHEMA_NAME")]
	[string] $Name = ""
}
