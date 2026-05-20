using namespace MySqlConnector
using namespace System.Diagnostics.CodeAnalysis

<#
.SYNOPSIS
	Creates a new MariaDB/MySQL database connection.
#>
function New-MySqlConnection {
	[CmdletBinding()]
	[OutputType([MySqlConnector.MySqlConnection])]
	[SuppressMessage("PSUseShouldProcessForStateChangingFunctions", "")]
	param (
		# The connection URI used to open the database.
		[Parameter(Mandatory, Position = 0, ValueFromPipeline)]
		[ValidateScript(
			{ $_.IsAbsoluteUri -and ($_.Scheme -in "mariadb", "mysql") -and $_.UserInfo.Contains(":") },
			ErrorMessage = "The specified connection URI is invalid."
		)]
		[uri] $Uri,

		# Value indicating whether to open the connection.
		[switch] $Open
	)

	process {
		$userName, $password = ($Uri.UserInfo -split ":").ForEach{ [Uri]::UnescapeDataString($_) }
		$builder = [MySqlConnectionStringBuilder]@{
			Server = $Uri.Host
			Port = $Uri.IsDefaultPort ? 3306 : $Uri.Port
			Database = "information_schema"
			UserID = $userName
			Password = $password
			ConvertZeroDateTime = $true
			Pooling = $false
			UseCompression = $Uri.Host -notin "::1", "127.0.0.1", "localhost"
		}

		New-SqlConnection ([MySqlConnection]) $builder.ConnectionString -Open:$Open
	}
}
