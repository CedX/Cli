namespace Belin.Cli.MySql

open Belin.Sql
open MySqlConnector
open System

/// Contains operations for working with database connections.
[<AutoOpen>]
module Connection =

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
    member this.GetCollations(): Collation seq =
      this.Query<Collation> "SHOW COLLATION"

    /// Gets the list of columns contained in the specified table.
    member this.GetColumns(table: Table): Column seq =
      let sql = "
        SELECT *
        FROM information_schema.COLUMNS
        WHERE TABLE_SCHEMA = @Schema AND TABLE_NAME = @Name
        ORDER BY ORDINAL_POSITION"

      this.Query<Column>(sql, ParameterCollection [
        "Name", table.Name :> objnull
        "Schema", table.Schema
      ])

    /// Gets the list of all storage engines.
    member this.GetEngines(): Engine seq =
      this.Query<Engine> "SHOW ENGINES"

    /// Gets the list of schemas hosted by a database server.
    member this.GetSchemas(): Schema seq =
      this.Query<Schema> "
        SELECT *
        FROM information_schema.SCHEMATA
        WHERE SCHEMA_NAME NOT IN ('information_schema', 'mysql', 'performance_schema', 'sys')
        ORDER BY SCHEMA_NAME"

    /// Gets the list of tables contained in the specified schema.
    member this.GetTables(schema: Schema): Table seq =
      let sql = "
        SELECT *
        FROM information_schema.TABLES
        WHERE TABLE_SCHEMA = @Name AND TABLE_TYPE = @Type
        ORDER BY TABLE_NAME"

      this.Query<Table>(sql, ParameterCollection [
        "Name", schema.Name :> objnull
        "Type", TableType.BaseTable
      ])
