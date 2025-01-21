using WhiteTale.Server.Domain.Rooms;

namespace WhiteTale.Server.IntegrationTests.Helpers;

internal sealed record SeedRoomConnectionResult
{
	internal required RoomConnectionSeed Seed { get; init; }

	internal required RoomConnection Connection { get; init; }
}
