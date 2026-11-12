namespace Belin.Cli.MySql

open System.ComponentModel.DataAnnotations.Schema

/// Represents a storage engine.
[<CLIMutable>]
type MySqlEngine = {

  /// The engine name.
  [<Column "Engine">]
  Name: string
}
