using namespace System.Diagnostics.CodeAnalysis
using module ./Schema.psm1

<#
.SYNOPSIS
	Alters the character set of MariaDB/MySQL tables.
.OUTPUTS
	The log messages.
#>
function Set-MySqlCharset {
	[CmdletBinding()]
	[OutputType([string])]
	[SuppressMessage("PSUseShouldProcessForStateChangingFunctions", "")]
	param (
		# The connection URI.
		[Parameter(Mandatory, Position = 0)]
		[uri] $Uri,

		# The name of the new character set.
		[Parameter(Mandatory, Position = 1)]
		[string] $Collation,

		# The schema name.
		[string[]] $Schema = @(),

		# The table name.
		[string[]] $Table = @()
	)

	begin {
		$connection = New-MySqlConnection $Uri
		$collations = Get-MySqlCollation $connection
		if ($Collation -notin $collations) { throw [ArgumentOutOfRangeException] "Collation" }
	}

	process {
		$schemas = $Schema ? $Schema.ForEach{ [Schema]@{ Name = $_ } } : @(Get-MySqlSchema $connection)
		$tables = foreach ($schemaObject in $schemas) {
			$Table ? $Table.ForEach{ [Table]@{ Name = $_; Schema = $schemaObject.Name } } : @(Get-MySqlTable $connection $schemaObject)
		}

		foreach ($tableObject in $tables) {
			"Processing: $($tableObject.QualifiedName())"
			$charset = ($Collation -split "_")[0]
			$sql = "
				SET foreign_key_checks = 0;
				ALTER TABLE $($tableObject.GetQualifiedName($true)) CONVERT TO CHARACTER SET $charset COLLATE $Collation;
				SET foreign_key_checks = 1;"

			Invoke-SqlNonQuery $connection -Command $sql | Out-Null
		}
	}

	clean {
		Close-SqlConnection $connection
	}
}
