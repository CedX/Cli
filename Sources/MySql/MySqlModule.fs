namespace Belin.Cli.MySql

open System.CommandLine

/// Manages the MariaDB/MySQL databases.
type MySqlModule(
  optimize: OptimizeCommand,
  setCharset: SetCharsetCommand,
  setEngine: SetEngineCommand) as this =

  inherit Command("mysql", "Manage the MariaDB/MySQL databases.")
  do
    this.Subcommands.Add optimize
    this.Subcommands.Add setCharset
    this.Subcommands.Add setEngine
