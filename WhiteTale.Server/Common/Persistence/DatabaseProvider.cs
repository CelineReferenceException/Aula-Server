using System.Text.Json.Serialization;

namespace WhiteTale.Server.Common.Persistence;

[JsonConverter(typeof(JsonStringEnumConverter<DatabaseProvider>))]
internal enum DatabaseProvider
{
	InMemory,
	Sqlite
}
