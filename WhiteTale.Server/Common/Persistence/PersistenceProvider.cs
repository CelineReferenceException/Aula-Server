using System.Text.Json.Serialization;

namespace WhiteTale.Server.Common.Persistence;

[JsonConverter(typeof(JsonStringEnumConverter<PersistenceProvider>))]
internal enum PersistenceProvider
{
	/// <summary>
	///     A temporal in-memory database which exists only during the application session.
	/// </summary>
	InMemory,
	Sqlite,
}
