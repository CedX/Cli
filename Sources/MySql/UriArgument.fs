namespace Belin.Cli.MySql

open System
open System.CommandLine
open System.CommandLine.Parsing

/// Provides the connection URI of a data source.
type UriArgument() as this =
  inherit Argument<string> "uri"
  do
    this.Description <- "The connection URI."
    this.Validators.Add this.Validate

  /// The list of supported schemes.
  static let allowedSchemes = [| "mariadb"; "mysql" |]

  /// Validates the result produced when parsing this argument.
  member private _.Validate(result: ArgumentResult) =
    let schemes = allowedSchemes |> Array.map (fun scheme -> $"'{scheme}'") |> String.concat " or "
    match Uri.TryCreate(result.GetRequiredValue this, UriKind.Absolute) with
    | false, _ -> result.AddError $@"The ""{this.Name}"" argument requires a valid connection URI."
    | true, uri ->
      let uri = nonNull uri
      if not (allowedSchemes |> Array.contains uri.Scheme) then result.AddError $@"The ""{this.Name}"" argument only supports the {schemes} scheme."
      elif not (uri.UserInfo.Contains ':') then result.AddError $@"The ""{this.Name}"" argument requires full credentials to be specified."
