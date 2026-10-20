namespace Belin.Cli.Cmdlets

open Belin.Cli
open Belin.Cli.MySql
open System
open System.Diagnostics
open System.IO
open System.Management.Automation
open System.Text
open System.Web

/// Restores a set of MariaDB/MySQL tables.
[<Cmdlet(VerbsData.Restore, "MySqlTable"); OutputType(typeof<Void>)>]
type RestoreMySqlTable() =
  inherit PSCmdlet()

  /// The connection URI.
  [<Parameter(Mandatory = true, Position = 1); ValidateConnectionUri>]
  member val Uri: Uri | null = null with get, set

  /// Specifies the path to an SQL dump.
  [<Parameter(Mandatory = true, ParameterSetName = "Path", Position = 2, ValueFromPipeline = true); SupportsWildcards>]
  member val Path: string array = [||] with get, set

  /// Specifies the literal path to an SQL dump.
  [<Parameter(Mandatory = true, ParameterSetName = "LiteralPath"); ValidatePath("The specified literal path is invalid.")>]
  member val LiteralPath: string array = [||] with get, set

  /// A pattern used to filter the list of files to be processed.
  [<Parameter>]
  member val Filter = "*.sql" with get, set

  /// Value indicating whether to process the input path recursively.
  [<Parameter>]
  member val Recurse = SwitchParameter false with get, set

  /// Performs execution of this command.
  override this.ProcessRecord() =
    let connectionUri = nonNull this.Uri
    let query = HttpUtility.ParseQueryString connectionUri.Query
    let userId, password =
      match connectionUri.UserInfo.Split(':', 2) with
      | [||] -> "root", ""
      | [| userId |] -> Uri.UnescapeDataString userId, ""
      | parts -> Uri.UnescapeDataString parts[0], Uri.UnescapeDataString parts[1]

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

    for file in files do
      let schema = Path.GetFileNameWithoutExtension file.Name
      InformationRecord($"Importing: {schema}", nameof this.ProcessRecord) |> this.WriteInformation

      let arguments = ResizeArray<string>([
        $"""--default-character-set={query["charset"] |> defaultIfNull "utf8mb4"}"""
        $"--execute=USE {schema}; SOURCE {file.FullName.Replace('\\', '/')};"
        $"--host={connectionUri.Host}"
        $"--password={password}"
        $"--port={if connectionUri.IsDefaultPort then 3306 else connectionUri.Port}"
        $"--user={userId}"
      ])

      if not (LocalHosts |> Array.contains connectionUri.Host) then arguments.Add "--compress"
      let startInfo = ProcessStartInfo("mysql", arguments, CreateNoWindow = true, RedirectStandardError = true)
      let message = $"{startInfo.FileName} {startInfo.Arguments}"

      match Process.Start startInfo with
      | null -> raise (ApplicationFailedException message)
      | startedProcess ->
        let stderr = startedProcess.StandardError.ReadToEnd().Trim()
        startedProcess.WaitForExit()
        if startedProcess.ExitCode <> 0 then raise (ApplicationFailedException stderr)
