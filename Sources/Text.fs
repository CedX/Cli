namespace Belin.Cli

open System
open System.IO
open System.Management.Automation
open System.Text
open System.Text.Json

/// Contains operations for working with text files.
module internal Text =

  /// The path to the resource directory.
  let private resources: string =
    typeof<Architecture>.Assembly.Location |> Path.GetDirectoryName |> fun name -> Path.Join(name, "../Resources/Text")

  /// The list of binary file extensions.
  let binaryExtensions: string array =
    Path.Join(resources, "BinaryExtensions.json") |> File.ReadAllText |> JsonSerializer.Deserialize<string array> |> nonNull

  /// The list of text file extensions.
  let textExtensions: string array =
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
  let readAllText (file: FileInfo): (byte array) option =
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

/// Converts the encoding of input files.
[<Cmdlet(VerbsData.ConvertTo, "Encoding", DefaultParameterSetName = "Path"); OutputType(typeof<string>)>]
type ConvertToEncoding() =
  inherit PSCmdlet()

  /// The path of the files to convert.
  [<Parameter(Mandatory = true, ParameterSetName = "Path", Position = 1, ValueFromPipeline = true); SupportsWildcards>]
  member val Path: string array = [||] with get, set

  /// The literal path of the files to convert.
  [<Parameter(Mandatory = true, ParameterSetName = "LiteralPath"); ValidatePath("The specified literal path is invalid.")>]
  member val LiteralPath: string array = [||] with get, set

  /// The destination encoding.
  [<Parameter(Mandatory = true); ValidateSet("Latin1", "UTF-8")>]
  member val Encoding = "UTF-8" with get, set

  /// The list of folders to exclude from the processing.
  [<Parameter; ValidateNotNull>]
  member val Exclude: string array = [| ".git"; "node_modules"; "obj"; "ps_modules"; "vendor" |] with get, set

  /// A pattern used to filter the list of files to be processed.
  [<Parameter>]
  member val Filter = "" with get, set

  /// Value indicating whether to process the input path recursively.
  [<Parameter>]
  member val Recurse = SwitchParameter false with get, set

  /// Performs execution of this command.
  override this.ProcessRecord () =
    let commandName = "ConvertTo-Encoding"
    let sourceEncoding = if this.Encoding.ToLowerInvariant() = "latin1" then Encoding.UTF8 else Encoding.Latin1
    let targetEncoding = Encoding.GetEncoding this.Encoding

    let scriptBlock = ScriptBlock.Create(string (StringBuilder("Get-ChildItem -File")
      .Append(if this.ParameterSetName = "Path" then " -Path $args[0]" else " -LiteralPath $args[0]")
      .Append(if this.Filter.Length > 0 then " -Filter $args[1]" else "")
      .Append(if this.Recurse.IsPresent then " -Recurse" else "")))

    let arguments = ResizeArray<obj>()
    arguments.Add(if this.ParameterSetName = "Path" then this.Path else this.LiteralPath)
    if this.Filter.Length > 0 then arguments.Add this.Filter

    let files =
      scriptBlock.Invoke(arguments.ToArray())
      |> Seq.map (fun psObject -> psObject.BaseObject :?> FileInfo)
      |> Seq.filter (fun file -> not (Text.isExcludedFile file this.Exclude))

    for file in files do
      match Text.readAllText file with
      | None -> ()
      | Some bytes ->
        InformationRecord($"Converting: {file}", commandName) |> this.WriteInformation
        File.WriteAllBytes(file.FullName, Encoding.Convert(sourceEncoding, targetEncoding, bytes))
