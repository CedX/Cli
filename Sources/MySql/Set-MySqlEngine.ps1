using namespace System.Diagnostics.CodeAnalysis
using module ./Schema.psm1

<#
.SYNOPSIS
	Alters the storage engine of MariaDB/MySQL tables.
.OUTPUTS
	The log messages.
#>
function Set-MySqlEngine {
	[CmdletBinding()]
	[OutputType([string])]
	[SuppressMessage("PSUseShouldProcessForStateChangingFunctions", "")]
	param (
		# The connection URI.
		[Parameter(Mandatory, Position = 0)]
		[uri] $Uri,

		# The name of the new storage engine.
		[Parameter(Mandatory, Position = 1)]
		[string] $Engine,

		# The schema name.
		[string[]] $Schema = @(),

		# The table name.
		[string[]] $Table = @()
	)

	begin {
		$connection = New-MySqlConnection $Uri
		$engines = Get-MySqlEngine $connection
		if ($Engine -notin $engines) { throw [ArgumentOutOfRangeException] "Engine" }
	}

	process {
		$schemas = $Schema ? $Schema.ForEach{ [Schema]@{ Name = $_ } } : @(Get-MySqlSchema $connection)
		$tables = foreach ($schemaObject in $schemas) {
			$Table ? $Table.ForEach{ [Table]@{ Name = $_; Schema = $schemaObject.Name } } : @(Get-MySqlTable $connection $schemaObject)
		}

		foreach ($tableObject in $tables) {
			"Processing: $($tableObject.QualifiedName())"
			$sql = "
				SET foreign_key_checks = 0;
				ALTER TABLE $($tableObject.GetQualifiedName($true)) ENGINE = $Engine;
				SET foreign_key_checks = 1;"

			Invoke-SqlNonQuery $connection -Command $sql | Out-Null
		}
	}

	clean {
		Close-SqlConnection $connection
	}
}
