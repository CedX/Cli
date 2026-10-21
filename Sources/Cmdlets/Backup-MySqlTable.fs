namespace Belin.Cli.Cmdlets

open Belin.Cli
open Belin.Cli.MySql
open MySqlConnector
open System
open System.Diagnostics
open System.IO
open System.Management.Automation
open System.Web

/// Backups a set of MariaDB/MySQL tables.
[<Cmdlet(VerbsData.Backup, "MySqlTable"); OutputType(typeof<Void>); Sealed>]
type BackupMySqlTable() =
  inherit Cmdlet()

  /// The connection URI.
  [<Parameter(Mandatory = true, Position = 1); ValidateConnectionUri>]
  member val Uri: Uri | null = null with get, set

  /// The path to the output directory.
  [<Parameter(Mandatory = true, Position = 2); ValidatePath("The specified output path is invalid.")>]
  member val Path = "" with get, set

  /// The schema name.
  [<Parameter>]
  member val Schema: string array = [||] with get, set

  /// The table name.
  [<Parameter>]
  member val Table: string array = [||] with get, set

  /// Performs execution of this command.
  override this.ProcessRecord() =
    let schemas =
      if not (Array.isEmpty this.Schema) then
        this.Schema |> Seq.map (fun schema -> MySqlSchema(Name = schema))
      else
        use connection = MySqlConnection.OfUri(nonNull this.Uri)
        let objects = InformationSchema.getSchemas connection
        connection.Close()
        objects

    let directory = Directory.CreateDirectory this.Path
    for schema in schemas do
      let tables = this.Table |> Array.map (fun table -> MySqlTable(Name = table, Schema = schema.Name))
      let object = if tables.Length = 1 then tables[0].QualifiedName else schema.Name
      InformationRecord($"Exporting: {object}", nameof this.ProcessRecord) |> this.WriteInformation
      this.ExportSqlDump(directory, schema, tables)

  /// Exports the specified schema to a SQL dump into the specified directory.
  member private this.ExportSqlDump(directory: DirectoryInfo, schema: MySqlSchema, tables: MySqlTable array) =
    let connectionUri = nonNull this.Uri
    let object = if tables.Length = 1 then tables[0].QualifiedName else schema.Name

    let query = HttpUtility.ParseQueryString connectionUri.Query
    let userId, password =
      match connectionUri.UserInfo.Split(':', 2) with
      | [||] -> "root", ""
      | [| userId |] -> Uri.UnescapeDataString userId, ""
      | parts -> Uri.UnescapeDataString parts[0], Uri.UnescapeDataString parts[1]

    let arguments = ResizeArray<string>([
      $"""--default-character-set={query["charset"] |> defaultIfNull "utf8mb4"}"""
      $"--host={connectionUri.Host}"
      $"--password={password}"
      $"--port={if connectionUri.IsDefaultPort then 3306 else connectionUri.Port}"
      $"--result-file={Path.Join(directory.FullName, object)}.sql"
      $"--user={userId}"
    ])

    if not (LocalHosts |> Array.contains connectionUri.Host) then arguments.Add "--compress"
    arguments.Add schema.Name
    arguments.AddRange (tables |> Array.map _.Name)

    let startInfo = ProcessStartInfo("mysqldump", arguments, CreateNoWindow = true, RedirectStandardError = true)
    match Process.Start startInfo with
    | null ->
      let ex = ApplicationFailedException $"{startInfo.FileName} {startInfo.Arguments}"
      ErrorRecord(ex, "ApplicationFailed", ErrorCategory.OperationStopped, startInfo) |> this.ThrowTerminatingError
    | startedProcess ->
      let stderr = startedProcess.StandardError.ReadToEnd().Trim()
      startedProcess.WaitForExit()
      if startedProcess.ExitCode <> 0 then
        let ex = ApplicationFailedException stderr
        ErrorRecord(ex, "ApplicationFailed", ErrorCategory.OperationStopped, startInfo) |> this.ThrowTerminatingError
