namespace Belin.Cli

open Belin.Sql
open MySqlConnector
open System
open System.Data
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

/// Represents a collation.
[<CLIMutable>]
type MySqlCollation = {

  /// The collation name.
  [<Column "Collation">]
  Name: string
}

/// Provides the metadata of a table column.
[<Table "COLUMNS"; CLIMutable>]
type MySqlColumn = {

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

/// Represents a storage engine.
[<CLIMutable>]
type MySqlEngine = {

  /// The engine name.
  [<Column "Engine">]
  Name: string
}

/// Provides the metadata of a database schema.
[<Table "SCHEMATA"; CLIMutable>]
type MySqlSchema = {

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

  /// The fully qualified name.
  member this.QualifiedName = this.GetQualifiedName(escape = false)

  /// Gets the fully qualified name.
  member this.GetQualifiedName(escape: bool) =
    let escapeFn = if escape then (fun value -> $"`{value}`") else id
    $"{escapeFn this.Schema}.{escapeFn this.Name}"

/// Contains operations for working with the MySQL information schema.
module InformationSchema =

  /// Gets the list of columns contained in the specified table.
  let getColumns (connection: IDbConnection, table: MySqlTable): MySqlColumn seq =
    let sql = "
      SELECT *
      FROM information_schema.COLUMNS
      WHERE TABLE_SCHEMA = @Schema AND TABLE_NAME = @Name
      ORDER BY ORDINAL_POSITION"

    connection.Query<MySqlColumn>(sql, ParameterCollection [
      "Name", table.Name :> objnull
      "Schema", table.Schema
    ])

  /// Gets the list of schemas hosted by a database server.
  let getSchemas (connection: IDbConnection): MySqlSchema seq =
    connection.Query<MySqlSchema> "
      SELECT *
      FROM information_schema.SCHEMATA
      WHERE SCHEMA_NAME NOT IN ('information_schema', 'mysql', 'performance_schema', 'sys')
      ORDER BY SCHEMA_NAME"

  /// Gets the list of tables contained in the specified schema.
  let getTables (connection: IDbConnection, schema: MySqlSchema): MySqlTable seq =
    let sql = "
      SELECT *
      FROM information_schema.TABLES
      WHERE TABLE_SCHEMA = @Name AND TABLE_TYPE = @Type
      ORDER BY TABLE_NAME"

    connection.Query<MySqlTable>(sql, ParameterCollection [
      "Name", schema.Name :> objnull
      "Type", MySqlTableType.BaseTable
    ])

/// Contains operations for working with MySQL databases.
module MySql =

  /// The list of local host addresses.
  let LocalHosts = [| "::1"; "127.0.0.1"; "localhost" |]

  /// Provides extension members for MySQL connections.
  type MySqlConnection with

    /// Creates a new MySQL connection from the specified connection URI.
    static member OfUri(connectionUri: Uri): MySqlConnection =
      let userId, password =
        match connectionUri.UserInfo.Split(':', 2) with
        | [||] -> "root", ""
        | [| userId |] -> Uri.UnescapeDataString userId, ""
        | parts -> Uri.UnescapeDataString parts[0], Uri.UnescapeDataString parts[1]

      let database = connectionUri.AbsolutePath.Trim '/'
      let builder = MySqlConnectionStringBuilder(
        Server = connectionUri.Host,
        Port = (if connectionUri.IsDefaultPort then 3306u else uint connectionUri.Port),
        Database = (if database.Length > 0 then database else "information_schema"),
        UserID = userId,
        Password = password,
        ConvertZeroDateTime = true,
        Pooling = false,
        UseCompression = not (LocalHosts |> Array.contains connectionUri.Host)
      )

      new MySqlConnection(string builder)

    /// Gets the list of all collations.
    member this.GetCollations(): string seq =
      this.Query<MySqlCollation> "SHOW COLLATION" |> Seq.map _.Name

    /// Gets the list of all storage engines.
    member this.GetEngines(): string seq =
      this.Query<MySqlEngine> "SHOW ENGINES" |> Seq.map _.Name
