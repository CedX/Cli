namespace Belin.Cli.Data;

using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// Provides the metadata of a database schema.
/// </summary>
[Table("SCHEMATA")]
public class MySqlSchema {

	/// <summary>
	/// The default character set.
	/// </summary>
	[Column("DEFAULT_CHARACTER_SET_NAME")]
	public string Charset { get; set; } = "";

	/// <summary>
	/// The default collation.
	/// </summary>
	[Column("DEFAULT_COLLATION_NAME")]
	public string Collation { get; set; } = "";

	/// <summary>
	/// The schema name.
	/// </summary>
	[Column("SCHEMA_NAME")]
	public string Name { get; set; } = "";
}
