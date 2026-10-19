namespace Belin.Cli

open Belin.Sql
open MySqlConnector
open System
open System.Collections.Generic
open System.Data
open System.ComponentModel.DataAnnotations.Schema

/// Contains operations for working with MySQL databases.
module MySql =

  /// The list of local host addresses.
  let private localHosts = [| "::1"; "127.0.0.1"; "localhost" |]

  /// Provides extension members for MySQL connections.
  type MySqlConnection with

    /// Creates a new MySQL connection from the specified connection URI.
    static member OfUri(connectionUri: Uri): MySqlConnection =
      let userId, password =
        match connectionUri.UserInfo.Split(':', 2) with
        | [||] -> "root", ""
        | [| userName |] -> Uri.UnescapeDataString userName, ""
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
        UseCompression = not (localHosts |> Array.contains connectionUri.Host)
      )

      new MySqlConnection(string builder)

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

/// Provides the metadata of a table column.
[<Table("COLUMNS")>]
type MySqlColumn() =

  /// The column name.
  [<Column("COLUMN_NAME")>]
  member val Name = "" with get, set

  /// The column position.
  [<Column("ORDINAL_POSITION")>]
  member val Position = 0 with get, set

  /// The schema containing this column.
  [<Column("TABLE_SCHEMA")>]
  member val Schema = "" with get, set

  /// The table containing this column.
  [<Column("TABLE_NAME")>]
  member val Table = "" with get, set

/// Provides the metadata of a database schema.
[<Table("SCHEMATA")>]
type MySqlSchema() =

  /// The default character set.
  [<Column("DEFAULT_CHARACTER_SET_NAME")>]
  member val Charset = "" with get, set

  /// The default collation.
  [<Column("DEFAULT_COLLATION_NAME")>]
  member val Collation = "" with get, set

  /// The schema name.
  [<Column("SCHEMA_NAME")>]
  member val Name = "" with get, set

/// Provides the metadata of a database table.
[<Table("TABLES")>]
type MySqlTable() =

  /// The default collation.
  [<Column("TABLE_COLLATION")>]
  member val Collation = "" with get, set

  /// The storage engine.
  [<Column("ENGINE")>]
  member val Engine = MySqlTableEngine.None with get, set

  /// The table name.
  [<Column("TABLE_NAME")>]
  member val Name = "" with get, set

  /// The fully qualified name.
  member this.QualifiedName = this.GetQualifiedName(escape = false)

  /// The schema containing this table.
  [<Column("TABLE_SCHEMA")>]
  member val Schema = "" with get, set

  /// The table type.
  [<Column("TABLE_TYPE")>]
  member val Type = MySqlTableType.BaseTable with get, set

  /// Gets the fully qualified name.
  member this.GetQualifiedName(escape: bool) =
    let escapeFn = if escape then (fun value -> $"`{value}`") else id
    $"{escapeFn this.Schema}.{escapeFn this.Name}"

/// Contains operations for working with the MySQL information schema.
module InformationSchema =

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

    connection.Query<MySqlTable>(sql, SqlParameterCollection([
      SqlParameter("Name", schema.Name)
      SqlParameter("Type", MySqlTableType.BaseTable)
    ]))
