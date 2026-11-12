namespace Belin.Cli.MySql

open Belin.Sql
open MySqlConnector
open System

/// Contains operations for working with MySQL databases.
module MySqlConnectionExtensions =

  /// Provides extension members for MySQL connections.
  type MySqlConnection with

    /// The list of local host addresses.
    static member LocalHosts = [| "::1"; "127.0.0.1"; "localhost" |]

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
        UseCompression = not (MySqlConnection.LocalHosts |> Array.contains connectionUri.Host)
      )

      new MySqlConnection(string builder)

    /// Gets the list of all collations.
    member this.GetCollations(): string seq =
      this.Query<MySqlCollation> "SHOW COLLATION" |> Seq.map _.Name

    /// Gets the list of all storage engines.
    member this.GetEngines(): string seq =
      this.Query<MySqlEngine> "SHOW ENGINES" |> Seq.map _.Name
