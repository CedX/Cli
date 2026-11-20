namespace Belin.Cli.Install

open System.CommandLine
open System.IO

/// Provides the identifier of a scheduled task.
type DestinationArgument(defaultValue: string) as this =
  inherit Argument<DirectoryInfo> "destination"
  do
    this.DefaultValueFactory <- fun _ -> DirectoryInfo defaultValue
    this.Description <- "The path to the destination directory."
    this.AcceptLegalFilePathsOnly() |> ignore<Argument<DirectoryInfo>>
