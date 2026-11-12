namespace Belin.Cli.MySql

open Belin.Sql
open System.Data

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
