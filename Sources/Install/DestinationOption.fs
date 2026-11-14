namespace Belin.Cli.Install

open System.CommandLine
open System.IO

/// Provides the path to a destination directory.
type DestinationOption(defaultValue: string) as this =
  inherit Option<DirectoryInfo>("--destination", [| "-d" |], HelpName = "path")
  do
    this.DefaultValueFactory <- fun _ -> DirectoryInfo defaultValue
    this.Description <- "The path to the destination directory."
    this.AcceptLegalFilePathsOnly() |> ignore<Option<DirectoryInfo>>
