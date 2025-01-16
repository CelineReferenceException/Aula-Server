using WhiteTale.Server.Domain.Rooms;

namespace WhiteTale.Server.IntegrationTests.Helpers;

internal sealed record SeedRoomResult
{
	internal required RoomSeed Seed { get; init; }

	internal required Room Room { get; init; }
}
