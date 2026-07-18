namespace Belin.Cli.MySql;

using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// Provides the metadata of a table column.
/// </summary>
[Table("COLUMNS")]
public class MySqlColumn {

	/// <summary>
	/// The column name.
	/// </summary>
	[Column("COLUMN_NAME")]
	public string Name { get; set; } = "";

	/// <summary>
	/// The column position.
	/// </summary>
	[Column("ORDINAL_POSITION")]
	public int Position { get; set; }

	/// <summary>
	/// The schema containing this column.
	/// </summary>
	[Column("TABLE_SCHEMA")]
	public string Schema { get; set; } = "";

	/// <summary>
	/// The table containing this column.
	/// </summary>
	[Column("TABLE_NAME")]
	public string Table { get; set; } = "";
}
