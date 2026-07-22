using module ./MySqlSchema.psm1

<#
.SYNOPSIS
	Backups a set of MariaDB/MySQL tables.
.OUTPUTS
	The log messages.
#>
function Backup-MySqlTable {
	[CmdletBinding()]
	[OutputType([string])]
	param (
		# The connection URI.
		[Parameter(Mandatory, Position = 1)]
		[uri] $Uri,

		# The path to the output directory.
		[Parameter(Mandatory, Position = 2)]
		[ValidateScript({ Test-Path $_ -IsValid }, ErrorMessage = "The specified output path is invalid.")]
		[string] $Path,

		# The schema name.
		[string[]] $Schema = @(),

		# The table name.
		[string[]] $Table = @()
	)

	begin {
		$connection = New-MySqlConnection $Uri
		New-Item $Path -Force -ItemType Directory | Out-Null
	}

	process {
		$schemas = $Schema ? $Schema.ForEach{ [MySqlSchema]@{ Name = $_ } } : @(Select-MySqlSchema $connection)
		foreach ($schemaObject in $schemas) {
			"Exporting: $($Table.Count -eq 1 ? "$($schemaObject.Name).$($Table[0])" : $schemaObject.Name)"
			Export-MySqlDump $schemaObject -Path $Path -Table $Table -Uri $Uri
		}
	}

	clean {
		Close-SqlConnection $connection -Dispose
	}
}
