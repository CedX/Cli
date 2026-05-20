using namespace System.Collections.Generic
using namespace System.IO
using namespace System.Web
using module ./Schema.psm1

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
		[Parameter(Mandatory, Position = 0)]
		[uri] $Uri,

		# The path to the output directory.
		[Parameter(Mandatory, Position = 1)]
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
		$schemas = $Schema ? $Schema.ForEach{ [Schema]@{ Name = $_ } } : @(Get-MySqlSchema $connection)
		foreach ($schemaObject in $schemas) {
			"Exporting: $($Table.Count -eq 1 ? "$($schemaObject.Name).$($Table[0])" : $schemaObject.Name)"
			Export-SqlDump $schemaObject -Path $Path -Table $Table -Uri $Uri
		}
	}

	clean {
		Close-SqlConnection $connection
	}
}

<#
.SYNOPSIS
	Exports the specified schema to a SQL dump in the specified directory.
#>
function Export-SqlDump {
	[CmdletBinding()]
	[OutputType([void])]
	param (
		# The database schema.
		[Parameter(Mandatory, Position = 0)]
		[Schema] $Schema,

		# The path to the output directory.
		[Parameter(Mandatory, Position = 1)]
		[ValidateScript({ Test-Path $_ -IsValid }, ErrorMessage = "The specified output path is invalid.")]
		[string] $Path,

		# The connection URI.
		[Parameter(Mandatory)]
		[uri] $Uri,

		# The table name.
		[string[]] $Table = @()
	)

	$file = "$($Table.Count -eq 1 ? "$($Schema.Name).$($Table[0])" : $Schema.Name).sql"
	$userName, $password = ($Uri.UserInfo -split ":").ForEach{ [Uri]::UnescapeDataString($_) }
	$arguments = [List[string]] @(
		"--default-character-set=$([HttpUtility]::ParseQueryString($Uri.Query)["charset"] ?? "utf8mb4")"
		"--host=$($Uri.Host)"
		"--password=$password"
		"--port=$($Uri.IsDefaultPort ? 3306 : $Uri.Port)"
		"--result-file=$(Join-Path $Path $file)"
		"--user=$userName"
	)

	if ($Uri.Host -notin "::1", "127.0.0.1", "localhost") { $arguments.Add("--compress") }
	$arguments.Add($Schema.Name)
	$arguments.AddRange($Table)
	mysqldump @arguments
}
