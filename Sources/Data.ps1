using namespace System.Collections.Generic
using namespace System.Diagnostics.CodeAnalysis
using namespace System.Web
using module ./Data/InformationSchema.psm1

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

<#
.SYNOPSIS
	Exports the specified schema to a SQL dump in the specified directory.
#>
function Export-MySqlDump {
	[CmdletBinding()]
	[OutputType([void])]
	param (
		# The database schema.
		[Parameter(Mandatory, Position = 1)]
		[MySqlSchema] $Schema,

		# The path to the output directory.
		[Parameter(Mandatory, Position = 2)]
		[ValidateScript({ Test-Path $_ -IsValid }, ErrorMessage = "The specified output path is invalid.")]
		[string] $Path,

		# The connection URI.
		[Parameter(Mandatory)]
		[uri] $Uri,

		# The table name.
		[string[]] $Table = @()
	)

	$file = "$($Table.Count -eq 1 ? "$($Schema.Name).$($Table[0])" : $Schema.Name).sql"
	$userName, $password = ($Uri.UserInfo -split ":").ForEach{ [uri]::UnescapeDataString($_) }
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
		$schemas = $Schema ? $Schema.ForEach{ [MySqlSchema]@{ Name = $_ } } : @(Select-MySqlSchema $connection)
		$tables = foreach ($schemaObject in $schemas) {
			$Table ? $Table.ForEach{ [MySqlTable]@{ Name = $_; Schema = $schemaObject.Name } } : @(Select-MySqlTable $connection $schemaObject)
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

<#
.SYNOPSIS
	Restores a set of MariaDB/MySQL tables.
.INPUTS
	The path to an SQL dump.
.OUTPUTS
	The log messages.
#>
function Restore-MySqlTable {
	[CmdletBinding(DefaultParameterSetName = "Path")]
	[OutputType([string])]
	param (
		# The connection URI.
		[Parameter(Mandatory, Position = 1)]
		[uri] $Uri,

		# Specifies the path to an SQL dump.
		[Parameter(Mandatory, ParameterSetName = "Path", Position = 2, ValueFromPipeline)]
		[SupportsWildcards()]
		[string[]] $Path,

		# Specifies the literal path to an SQL dump.
		[Parameter(Mandatory, ParameterSetName = "LiteralPath")]
		[ValidateScript({ Test-Path $_ -IsValid }, ErrorMessage = "The specified literal path is invalid.")]
		[string[]] $LiteralPath,

		# A pattern used to filter the list of files to be processed.
		[string] $Filter = "*.sql",

		# Value indicating whether to process the input path recursively.
		[switch] $Recurse
	)

	process {
		$parameters = @{ File = $true; Recurse = $Recurse }
		if ($Filter) { $parameters.Filter = $Filter }

		$files = $LiteralPath ? (Get-ChildItem -LiteralPath $LiteralPath @parameters) : (Get-ChildItem $Path @parameters)
		foreach ($file in $files) {
			"Importing: $($file.BaseName)"
			$userName, $password = ($Uri.UserInfo -split ":").ForEach{ [uri]::UnescapeDataString($_) }
			$arguments = [List[string]] @(
				"--default-character-set=$([HttpUtility]::ParseQueryString($Uri.Query)["charset"] ?? "utf8mb4")"
				"--execute=USE $($file.BaseName); SOURCE $($file.FullName -replace "\", "/");"
				"--host=$($Uri.Host)"
				"--password=$password"
				"--port=$($Uri.IsDefaultPort ? 3306 : $Uri.Port)"
				"--user=$userName"
			)

			if ($Uri.Host -notin "::1", "127.0.0.1", "localhost") { $arguments.Add("--compress") }
			mysql @arguments
		}
	}
}

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
		[Parameter(Mandatory, Position = 1)]
		[uri] $Uri,

		# The name of the new character set.
		[Parameter(Mandatory, Position = 2)]
		[string] $Collation,

		# The schema name.
		[string[]] $Schema = @(),

		# The table name.
		[string[]] $Table = @()
	)

	begin {
		$connection = New-MySqlConnection $Uri
		$collations = Select-MySqlCollation $connection
		if ($Collation -notin $collations) { throw [ArgumentOutOfRangeException]::new("Collation") }
	}

	process {
		$schemas = $Schema ? $Schema.ForEach{ [MySqlSchema]@{ Name = $_ } } : @(Select-MySqlSchema $connection)
		$tables = foreach ($schemaObject in $schemas) {
			$Table ? $Table.ForEach{ [MySqlTable]@{ Name = $_; Schema = $schemaObject.Name } } : @(Select-MySqlTable $connection $schemaObject)
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
		Close-SqlConnection $connection -Dispose
	}
}

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
		[Parameter(Mandatory, Position = 1)]
		[uri] $Uri,

		# The name of the new storage engine.
		[Parameter(Mandatory, Position = 2)]
		[string] $Engine,

		# The schema name.
		[string[]] $Schema = @(),

		# The table name.
		[string[]] $Table = @()
	)

	begin {
		$connection = New-MySqlConnection $Uri
		$engines = Select-MySqlEngine $connection
		if ($Engine -notin $engines) { throw [ArgumentOutOfRangeException]::new("Engine") }
	}

	process {
		$schemas = $Schema ? $Schema.ForEach{ [MySqlSchema]@{ Name = $_ } } : @(Select-MySqlSchema $connection)
		$tables = foreach ($schemaObject in $schemas) {
			$Table ? $Table.ForEach{ [MySqlTable]@{ Name = $_; Schema = $schemaObject.Name } } : @(Select-MySqlTable $connection $schemaObject)
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
		Close-SqlConnection $connection -Dispose
	}
}
