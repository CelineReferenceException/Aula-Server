namespace WhiteTale.Server.Common.Persistence;

internal sealed class PersistenceOptions
{
	internal const String SectionName = "Persistence";

	public String ConnectionString { get; set; } = "DataSource=./Persistence.db";

	public PersistenceProvider Provider { get; set; } = PersistenceProvider.Sqlite;
}
