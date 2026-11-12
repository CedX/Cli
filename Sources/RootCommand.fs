namespace Belin.Cli

open Belin.Cli.Commands
open System.CommandLine

/// Downloads and installs a predefined program.
type InstallCommand(gitea: InstallGitea, jdk: InstallJdk, node: InstallNode, nssm: InstallNssm, php: InstallPhp, powershell: InstallPowerShell) as this =
  inherit Command("install", "Install a predefined program.")
  do
    this.Subcommands.Add gitea
    this.Subcommands.Add jdk
    this.Subcommands.Add node
    this.Subcommands.Add nssm
    this.Subcommands.Add php
    this.Subcommands.Add powershell

/// Manages the MariaDB/MySQL databases.
type MySqlCommand(optimize: MySqlOptimize) as this =
  inherit Command("mysql", "Manage the MariaDB/MySQL databases.")
  do ()

/// The root command.
type RootCommand(install: InstallCommand) as this =
  inherit System.CommandLine.RootCommand "Common administrative tasks, such as database management, service management and software installation."
  do
    this.Subcommands.Add install
