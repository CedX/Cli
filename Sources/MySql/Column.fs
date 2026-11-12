namespace Belin.Cli.MySql

open System.ComponentModel.DataAnnotations.Schema

/// Provides the metadata of a table column.
[<Table "COLUMNS"; CLIMutable>]
type MySqlColumn =
  {
    /// The column name.
    [<Column "COLUMN_NAME">]
    Name: string

    /// The column position.
    [<Column "ORDINAL_POSITION">]
    Position: int

    /// The schema containing this column.
    [<Column "TABLE_SCHEMA">]
    Schema: string

    /// The table containing this column.
    [<Column "TABLE_NAME">]
    Table: string
  }

  /// Creates a new instance with default values.
  static member Default = {
    Name = ""
    Position = 0
    Schema = ""
    Table = ""
  }
