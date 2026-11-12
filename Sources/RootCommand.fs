namespace Belin.Cli

open Belin.Cli.Install
open Belin.Cli.MySql

/// The root command.
type RootCommand(install: InstallModule, mysql: MySqlModule) as this =
  inherit System.CommandLine.RootCommand "Common administrative tasks, such as database management, service management and software installation."
  do
    this.Subcommands.Add install
    this.Subcommands.Add mysql
