namespace Belin.Cli.Cmdlets

open Belin.Cli
open System
open System.IO
open System.Management.Automation
open System.Text

/// Converts the encoding of input files.
[<Cmdlet(VerbsData.ConvertTo, "Encoding", DefaultParameterSetName = "Path"); OutputType(typeof<Void>)>]
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
  override this.ProcessRecord() =
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
      match Text.readAllBytes file with
      | None -> ()
      | Some bytes ->
        InformationRecord($"Converting: {file}", nameof this.ProcessRecord) |> this.WriteInformation
        File.WriteAllBytes(file.FullName, Encoding.Convert(sourceEncoding, targetEncoding, bytes))
