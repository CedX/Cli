namespace Belin.Cli

open System
open System.IO
open System.Text.Json

/// Contains operations for working with text files.
module Text =

  /// The path to the resource directory.
  let private resources: string =
    typeof<Architecture>.Assembly.Location |> Path.GetDirectoryName |> fun name -> Path.Join(name, "../Resources/Text")

  /// The list of binary file extensions.
  let private binaryExtensions: string array =
    Path.Join(resources, "BinaryExtensions.json") |> File.ReadAllText |> JsonSerializer.Deserialize<string array> |> nonNull

  /// The list of text file extensions.
  let private textExtensions: string array =
    Path.Join(resources, "TextExtensions.json") |> File.ReadAllText |> JsonSerializer.Deserialize<string array> |> nonNull

  /// Returns a value indicating whether the specified file should be excluded from the processing.
  let isExcludedFile (file: FileInfo) (excludedFolders: string array): bool =
    let mutable directory = file.Directory
    let mutable isExcluded = false

    while not isExcluded && not (isNull directory) do
      let folder = nonNull directory
      directory <- folder.Parent
      isExcluded <- excludedFolders |> Array.contains folder.Name

    isExcluded

  /// Reads the contents of the specified text file into a byte array.
  /// Returns `None` if the given file is a binary file.
  let readAllBytes (file: FileInfo): (byte array) option =
    let extension = file.Extension.ToLowerInvariant()
    if extension.Length > 0 && binaryExtensions |> Array.contains extension then
      None
    elif extension.Length > 0 && textExtensions |> Array.contains extension then
      let bytes = File.ReadAllBytes file.FullName
      if bytes.Length > 0 then Some bytes else None
    else
      match File.ReadAllBytes file.FullName with
      | [||] -> None
      | bytes when Array.IndexOf(bytes, 0uy, 0, Math.Min(bytes.Length, 8_000)) < 0 -> Some bytes
      | _ -> None
