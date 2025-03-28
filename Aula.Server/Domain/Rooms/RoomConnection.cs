namespace Aula.Server.Domain.Rooms;

internal sealed class RoomConnection : DefaultDomainEntity
{
	private readonly Room? _sourceRoom;
	private readonly Room? _targetRoom;

	private RoomConnection(Snowflake id, Snowflake sourceRoomId, Snowflake targetRoomId)
	{
		Id = id;
		SourceRoomId = sourceRoomId;
		TargetRoomId = targetRoomId;
	}

	internal Snowflake Id { get; }

	internal Snowflake SourceRoomId { get; }

	internal Snowflake TargetRoomId { get; }

	// Navigation property
	internal Room SourceRoom
	{
		get => _sourceRoom ?? throw new InvalidOperationException($"{nameof(SourceRoom)} is null");
		init => _sourceRoom = value;
	}

	// Navigation property
	internal Room TargetRoom
	{
		get => _targetRoom ?? throw new InvalidOperationException($"{nameof(TargetRoom)} is null");
		init => _targetRoom = value;
	}

	internal static Result<RoomConnection, Object?> Create(Snowflake id, Snowflake sourceRoomId, Snowflake targetRoomId)
	{
		var roomConnection = new RoomConnection(id, sourceRoomId, targetRoomId);
		roomConnection.Events.Add(new RoomConnectionCreatedEvent(roomConnection));
		return roomConnection;
	}

	internal void Remove()
	{
		if (Events.Any(e => e is RoomConnectionRemovedEvent))
		{
			return;
		}

		Events.Add(new RoomConnectionRemovedEvent(this));
	}
}
