using System.Diagnostics.CodeAnalysis;

namespace Aula.Server.Domain.Messages;

internal sealed record MessageUserLeave
{
	internal UInt64 MessageId { get; }

	// Navigation property, values are set through reflection.
	[SuppressMessage("ReSharper", "UnassignedGetOnlyAutoProperty")]
	internal Message Message { get; } = null!;

	internal UInt64 UserId { get; }

	internal UInt64? RoomId { get; }

	internal MessageUserLeave(UInt64 messageId, UInt64 userId, UInt64? roomId)
	{
		if (messageId is 0)
		{
			throw new ArgumentException($"{nameof(messageId)} cannot be 0.", nameof(messageId));
		}

		if (userId is 0)
		{
			throw new ArgumentException($"{nameof(userId)} cannot be 0.", nameof(userId));
		}

		if (roomId is 0)
		{
			throw new ArgumentException($"{nameof(roomId)} cannot be 0.", nameof(roomId));
		}

		MessageId = messageId;
		UserId = userId;
		RoomId = roomId;
	}
}
