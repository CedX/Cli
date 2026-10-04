namespace Belin.Cli

open System.IO
open System.Linq
open System.Management.Automation

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

/// Validates that the specified path is valid.
type ValidatePathAttribute(errorMessage: string) =
  inherit ValidateArgumentsAttribute()

  /// An array containing the characters that are not allowed in path names.
  static let invalidCharacters = Path.GetInvalidPathChars()

  /// Verifies that the value of `arguments` is valid.
  override _.Validate(arguments: obj, _: EngineIntrinsics) =
    let isValid =
      match arguments with
      | :? string as path -> invalidCharacters.All (fun character -> not (path.Contains character))
      | _ -> false

    if not isValid then raise (ValidationMetadataException errorMessage)
