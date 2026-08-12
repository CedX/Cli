using namespace System.Diagnostics.CodeAnalysis
using module ./MySqlSchema.psm1

<#
.SYNOPSIS
	Creates a new MariaDB/MySQL schema.
.OUTPUTS
	The newly created schema.
#>
function New-MySqlSchema {
	[CmdletBinding()]
	[OutputType([MySqlSchema])]
	[SuppressMessage("PSUseShouldProcessForStateChangingFunctions", "")]
	param (
		# The schema name.
		[Parameter(Mandatory, Position = 1)]
		[string] $Name,

		# The default character set.
		[string] $Charset = "",

		# The default collation.
		[string] $Collation = ""
	)

	[MySqlSchema]@{
		Charset = $Charset
		Collation = $Collation
		Name = $Name
	}
}
