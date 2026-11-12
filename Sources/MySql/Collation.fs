namespace Belin.Cli.MySql

open System.ComponentModel.DataAnnotations.Schema

/// Represents a collation.
[<CLIMutable>]
type Collation = {

  /// The collation name.
  [<Column "Collation">]
  Name: string
}
