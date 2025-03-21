using System.Diagnostics.CodeAnalysis;

namespace Aula.Server.Domain.Rooms;

internal sealed class RoomConnection : DefaultDomainEntity
{
	internal UInt64 Id { get; }

	internal UInt64 SourceRoomId { get; }

	// Navigation property, values are set through reflection.
	[MaybeNull]
	[SuppressMessage("ReSharper", "UnassignedGetOnlyAutoProperty")]
	internal Room SourceRoom { get; }

	internal UInt64 TargetRoomId { get; }

	// Navigation property, values are set through reflection.
	[MaybeNull]
	[SuppressMessage("ReSharper", "UnassignedGetOnlyAutoProperty")]
	internal Room TargetRoom { get; }

	internal RoomConnection(UInt64 id, UInt64 sourceRoomId, UInt64 targetRoomId)
	{
		if (id is 0)
		{
			throw new ArgumentException($"{nameof(id)} cannot be 0.", nameof(id));
		}

		if (sourceRoomId is 0)
		{
			throw new ArgumentException($"{nameof(sourceRoomId)} cannot be 0.", nameof(sourceRoomId));
		}

		if (targetRoomId is 0)
		{
			throw new ArgumentException($"{nameof(targetRoomId)} cannot be 0.", nameof(targetRoomId));
		}

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
