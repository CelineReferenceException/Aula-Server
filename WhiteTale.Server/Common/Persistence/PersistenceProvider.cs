using System.Text.Json.Serialization;

namespace WhiteTale.Server.Common.Persistence;

[JsonConverter(typeof(JsonStringEnumConverter<PersistenceProvider>))]
internal enum PersistenceProvider
{
	InMemory,
	Sqlite,
}
