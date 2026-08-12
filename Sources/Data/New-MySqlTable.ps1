using namespace System.Diagnostics.CodeAnalysis
using module ./MySqlTable.psm1

<#
.SYNOPSIS
	Creates a new MariaDB/MySQL table.
.OUTPUTS
	The newly created table.
#>
function New-MySqlTable {
	[CmdletBinding()]
	[OutputType([MySqlTable])]
	[SuppressMessage("PSUseShouldProcessForStateChangingFunctions", "")]
	param (
		# The schema containing the table.
		[Parameter(Mandatory, Position = 1)]
		[string] $Schema,

		# The table name.
		[Parameter(Mandatory, Position = 2)]
		[string] $Name,

		# The default collation.
		[string] $Collation = "",

		# The storage engine.
		[string] $Engine = [MySqlTableEngine]::None,

		# The table type.
		[string] $Type = [MySqlTableType]::BaseTable
	)

	[MySqlTable]@{
		Collation = $Collation
		Engine = $Engine
		Name = $Name
		Schema = $Schema
		Type = $Type
	}
}
