namespace Belin.Cli

open Belin.Cli.Install
open Belin.Cli.MySql
open System.CommandLine

/// Downloads and installs a predefined program.
type InstallCommand(
  gitea: InstallGiteaCommand,
  jdk: InstallJdkCommand,
  node: InstallNodeCommand,
  nssm: InstallNssmCommand,
  php: InstallPhpCommand,
  powershell: InstallPowerShellCommand) as this =

  inherit Command("install", "Install a predefined program.")
  do
    this.Subcommands.Add gitea
    this.Subcommands.Add jdk
    this.Subcommands.Add node
    this.Subcommands.Add nssm
    this.Subcommands.Add php
    this.Subcommands.Add powershell

/// Manages the MariaDB/MySQL databases.
type MySqlCommand(
  optimize: MySqlOptimizeCommand) as this =

  inherit Command("mysql", "Manage the MariaDB/MySQL databases.")
  do
    this.Subcommands.Add optimize

/// The root command.
type RootCommand(install: InstallCommand, mysql: MySqlCommand) as this =
  inherit System.CommandLine.RootCommand "Common administrative tasks, such as database management, service management and software installation."
  do
    this.Subcommands.Add install
    this.Subcommands.Add mysql
