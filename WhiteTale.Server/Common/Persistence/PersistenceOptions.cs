namespace WhiteTale.Server.Common.Persistence;

internal sealed class PersistenceOptions
{
	internal const String SectionName = "Persistence";

	public String ConnectionString { get; set; } = "DataSource=./WhiteTale.Server.db";

	public DatabaseProvider Provider { get; set; } = DatabaseProvider.Sqlite;
}
