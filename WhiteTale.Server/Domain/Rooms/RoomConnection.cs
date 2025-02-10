namespace WhiteTale.Server.Domain.Rooms;

internal sealed class RoomConnection : DefaultDomainEntity
{
	private RoomConnection()
	{
	}

	internal UInt64 Id { get; private init; }

	internal UInt64 SourceRoomId { get; private init; }

	internal Room SourceRoom { get; private init; }

	internal UInt64 TargetRoomId { get; private init; }

	internal Room TargetRoom { get; private init; }

	internal static RoomConnection Create(UInt64 id, UInt64 sourceRoomId, UInt64 targetRoomId)
	{
		var connection = new RoomConnection
		{
			Id = id,
			SourceRoomId = sourceRoomId,
			TargetRoomId = targetRoomId,
		};

		connection.AddEvent(new RoomConnectionCreatedEvent(connection));
		return connection;
	}

	internal void Remove()
	{
		AddEvent(new RoomConnectionRemovedEvent(this));
	}
}
