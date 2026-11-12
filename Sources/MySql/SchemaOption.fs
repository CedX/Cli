namespace Belin.Cli.MySql

open System.CommandLine

/// Provides the name of a database schema.
type SchemaOption() =
  inherit Option<string array>(
    "--schema", [| "-s" |],
    HelpName = "name",
    Description = "The schema name."
  )
