namespace Belin.Cli.Install

open System.CommandLine

/// Downloads and installs a predefined program.
type InstallModule(
  gitea: GiteaCommand,
  jdk: JdkCommand,
  node: NodeCommand,
  nssm: NssmCommand,
  php: PhpCommand,
  powershell: PowerShellCommand) as this =

  inherit Command("install", "Install a predefined program.")
  do
    this.Subcommands.Add gitea
    this.Subcommands.Add jdk
    this.Subcommands.Add node
    this.Subcommands.Add nssm
    this.Subcommands.Add php
    this.Subcommands.Add powershell
