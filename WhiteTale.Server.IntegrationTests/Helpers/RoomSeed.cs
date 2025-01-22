namespace WhiteTale.Server.IntegrationTests.Helpers;

internal sealed record RoomSeed
{
	internal static RoomSeed Default { get; } = new()
	{
		Id = 1,
		Name = "Test Room",
		IsEntrance = false,
	};

	internal required UInt64 Id { get; init; }

	internal required String Name { get; init; }

	internal String? Description { get; init; }

	internal Boolean IsEntrance { get; init; }
}
