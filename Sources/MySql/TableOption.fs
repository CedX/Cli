namespace Belin.Cli.MySql

open System.CommandLine

/// Provides the name of a database table.
type TableOption() =
  inherit Option<string array>(
    "--table", [| "-t" |],
    HelpName = "name",
    Description = "The table name."
  )
