namespace Belin.Cli.MySql

open System.ComponentModel.DataAnnotations.Schema

/// Provides the metadata of a database schema.
[<Table "SCHEMATA"; CLIMutable>]
type MySqlSchema =
  {
    /// The default character set.
    [<Column "DEFAULT_CHARACTER_SET_NAME">]
    Charset: string

    /// The default collation.
    [<Column "DEFAULT_COLLATION_NAME">]
    Collation: string

    /// The schema name.
    [<Column "SCHEMA_NAME">]
    Name: string
  }

  /// Creates a new instance with default values.
  static member Default = {
    Charset = ""
    Collation = ""
    Name = ""
  }
