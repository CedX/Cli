namespace Belin.Cli

open System
open System.IO
open System.Management.Automation

/// Validates that the specified URI provides a valid connection string.
type ValidateConnectionUriAttribute() =
  inherit ValidateArgumentsAttribute()

  /// Verifies that the value of `arguments` is valid.
  override _.Validate(arguments: obj, _: EngineIntrinsics) =
    let isValid =
      match arguments with
      | :? Uri as uri -> uri.IsAbsoluteUri && (uri.Scheme = "mariadb" || uri.Scheme = "mysql")
      | _ -> false

    if not isValid then raise (ValidationMetadataException "The specified connection URI is invalid.")

/// Validates that the specified path is an existing file.
type ValidateFileAttribute(errorMessage: string) =
  inherit ValidateArgumentsAttribute()

  /// Verifies that the value of `arguments` is valid.
  override _.Validate(arguments: obj, _: EngineIntrinsics) =
    let exists =
      match arguments with
      | :? string as path -> File.Exists path
      | _ -> false

    if not exists then raise (ValidationMetadataException errorMessage)

/// Validates that the specified path does not contain any invalid character.
type ValidatePathAttribute(errorMessage: string) =
  inherit ValidateArgumentsAttribute()

  /// An array containing the characters that are not allowed in path names.
  static let invalidCharacters = Path.GetInvalidPathChars()

  /// Verifies that the value of `arguments` is valid.
  override _.Validate(arguments: obj, _: EngineIntrinsics) =
    let isValid =
      match arguments with
      | :? string as path -> not (invalidCharacters |> Array.exists path.Contains)
      | _ -> false

    if not isValid then raise (ValidationMetadataException errorMessage)
