namespace WhiteTale.Server.IntegrationTests.Helpers;

internal sealed record RoomConnectionSeed
{
	internal static RoomConnectionSeed Default { get; } = new()
	{
		Id = 1,
		SourceRoomId = 1,
		TargetRoomId = 2,
	};

	internal required UInt64 Id { get; init; }

	internal required UInt64 SourceRoomId { get; init; }

	internal required UInt64 TargetRoomId { get; init; }
}
