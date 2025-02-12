using System.Diagnostics.CodeAnalysis;

namespace Aula.Server.Domain.Messages;

internal sealed record MessageUserLeave
{
	internal required UInt64 MessageId { get; init; }

	// Navigation property, values are set through reflection.
	[SuppressMessage("ReSharper", "UnassignedGetOnlyAutoProperty")]
	internal Message Message { get; } = null!;

	internal required UInt64 UserId { get; init; }

	internal UInt64? RoomId { get; init; }
}
