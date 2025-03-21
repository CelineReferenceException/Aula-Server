using System.Diagnostics.CodeAnalysis;

namespace Aula.Server.Domain.Rooms;

internal sealed class RoomConnection : DefaultDomainEntity
{
	internal Snowflake Id { get; }

	internal Snowflake SourceRoomId { get; }

	// Navigation property, values are set through reflection.
	[MaybeNull]
	[SuppressMessage("ReSharper", "UnassignedGetOnlyAutoProperty")]
	internal Room SourceRoom { get; }

	internal Snowflake TargetRoomId { get; }

	// Navigation property, values are set through reflection.
	[MaybeNull]
	[SuppressMessage("ReSharper", "UnassignedGetOnlyAutoProperty")]
	internal Room TargetRoom { get; }

	internal RoomConnection(Snowflake id, Snowflake sourceRoomId, Snowflake targetRoomId)
	{
		Id = id;
		SourceRoomId = sourceRoomId;
		TargetRoomId = targetRoomId;

		AddEvent(new RoomConnectionCreatedEvent(this));
	}

	internal void Remove()
	{
		AddEvent(new RoomConnectionRemovedEvent(this));
	}
}
