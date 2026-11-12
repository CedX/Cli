namespace Belin.Cli.MySql

open System
open System.CommandLine
open System.CommandLine.Parsing

/// Provides the name of a database schema.
type SchemaOption() =
  inherit Option<string array>(
    "--schema", [| "-s" |],
    HelpName = "name",
    Description = "The schema name."
  )

/// Provides the name of a database table.
type TableOption() =
  inherit Option<string array>(
    "--table", [| "-t" |],
    HelpName = "name",
    Description = "The table name."
  )

/// Provides the connection string of a data source.
type UriOption() as this =
  inherit Option<string>("--uri", [| "-u" |], HelpName = "dsn")
  do
    this.Description <- "The connection URI."
    this.Required <- true
    this.Validators.Add this.Validate

  /// The list of supported schemes.
  static let allowedSchemes = [| "mariadb"; "mysql" |]

  /// Validates the result produced when parsing this option.
  member private _.Validate(result: OptionResult) =
    let schemes = allowedSchemes |> Array.map (fun scheme -> $"'{scheme}'") |> String.concat " or "
    match Uri.TryCreate(result.GetValue this, UriKind.Absolute) with
    | false, _ -> result.AddError $"The '--{this.Name}' option requires a valid connection URI."
    | true, uri ->
      let uri = nonNull uri
      if not (allowedSchemes |> Array.contains uri.Scheme) then result.AddError $"The '--{this.Name}' option only supports the {schemes} scheme."
      elif not (uri.UserInfo.Contains ':') then result.AddError $"The '--{this.Name}' option requires full credentials to be specified."
