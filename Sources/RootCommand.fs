namespace Belin.Cli

open Belin.Cli.Commands
open System.CommandLine

/// Downloads and installs a predefined program.
type InstallCommand(gitea: InstallGitea) as this =
  inherit Command("install", "Download and install a predefined program.")
  do
    this.Subcommands.Add gitea

/// The root command.
type RootCommand(install: InstallCommand) as this =
  inherit System.CommandLine.RootCommand "Common administrative tasks, such as database management, service management and software installation."
  do
    this.Subcommands.Add install
