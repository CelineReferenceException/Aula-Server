namespace WhiteTale.Server.Domain.Rooms;

internal sealed class RoomConnection : DefaultDomainEntity
{
	public UInt64 Id { get; private init; }

	public UInt64 SourceRoomId { get; private init; }

	public UInt64 TargetRoomId { get; private init; }

	private RoomConnection()
	{
	}

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
