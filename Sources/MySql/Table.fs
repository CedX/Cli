namespace Belin.Cli.MySql

open System.ComponentModel.DataAnnotations.Schema

/// Defines the storage engine of a table.
module MySqlTableEngine =

  /// The table does not use any storage engine.
  [<Literal>]
  let None = ""

  /// The storage engine is Aria.
  [<Literal>]
  let Aria = "Aria"

  /// The storage engine is InnoDB.
  [<Literal>]
  let InnoDB = "InnoDB"

  /// The storage engine is MyISAM.
  [<Literal>]
  let MyISAM = "MyISAM"

/// Defines the type of a table.
module MySqlTableType =

  /// A base table.
  [<Literal>]
  let BaseTable = "BASE TABLE"

  /// A view.
  [<Literal>]
  let View = "VIEW"

/// Provides the metadata of a database table.
[<Table "TABLES"; CLIMutable>]
type MySqlTable =
  {
    /// The default collation.
    [<Column "TABLE_COLLATION">]
    Collation: string

    /// The storage engine.
    [<Column "ENGINE">]
    Engine: string

    /// The table name.
    [<Column "TABLE_NAME">]
    Name: string

    /// The schema containing this table.
    [<Column "TABLE_SCHEMA">]
    Schema: string

    /// The table type.
    [<Column "TABLE_TYPE">]
    Type: string
  }

  /// Creates a new instance with default values.
  static member Default = {
    Collation = ""
    Engine = MySqlTableEngine.None
    Name = ""
    Schema = ""
    Type = MySqlTableType.BaseTable
  }

  /// The fully qualified name.
  member this.QualifiedName = this.GetQualifiedName(escape = false)

  /// Gets the fully qualified name.
  member this.GetQualifiedName(escape: bool) =
    let escapeFn = if escape then (fun value -> $"`{value}`") else id
    $"{escapeFn this.Schema}.{escapeFn this.Name}"
