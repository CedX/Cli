<#
.SYNOPSIS
	Optimizes a set of MariaDB/MySQL tables.
.OUTPUTS
	The log messages.
#>
function Optimize-MySqlTable {
	[CmdletBinding()]
	[OutputType([string])]
	param (
		# The connection URI.
		[Parameter(Mandatory, Position = 1)]
		[uri] $Uri,

		# The schema name.
		[string[]] $Schema = @(),

		# The table name.
		[string[]] $Table = @()
	)

	begin {
		$connection = New-MySqlConnection $Uri
	}

	process {
		$schemas = $Schema ? $Schema.ForEach{ New-MySqlSchema $_ } : @(Select-MySqlSchema $connection)
		$tables = foreach ($schemaObject in $schemas) {
			$Table ? $Table.ForEach{ New-MySqlTable -Schema $schemaObject.Name -Name $_ } : @(Select-MySqlTable $connection $schemaObject)
		}

		foreach ($tableObject in $tables) {
			"Optimizing: $($tableObject.QualifiedName())"
			Invoke-SqlNonQuery $connection -Command "OPTIMIZE TABLE $($tableObject.GetQualifiedName($true))" | Out-Null
		}
	}

	clean {
		Close-SqlConnection $connection -Dispose
	}
}
