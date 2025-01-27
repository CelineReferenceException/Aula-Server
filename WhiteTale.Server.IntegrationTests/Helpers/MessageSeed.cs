using WhiteTale.Server.Domain.Messages;

namespace WhiteTale.Server.IntegrationTests.Helpers;

internal sealed record MessageSeed
{
	internal static MessageSeed StandardTypeDefault { get; } = new()
	{
		Id = 1,
		Type = MessageType.Standard,
		Flags = 0,
		Target = MessageTarget.Room,
		Content = "Hello world",
		AuthorType = MessageAuthor.User,
		AuthorId = 1,
		TargetId = 1,
	};

	internal required UInt64 Id { get; init; }

	internal required MessageType Type { get; init; }

	internal required MessageFlags Flags { get; init; }

	internal required MessageTarget Target { get; init; }

	internal String? Content { get; init; }

	internal required MessageAuthor AuthorType { get; init; }

	internal required UInt64 AuthorId { get; init; }

	internal required UInt64 TargetId { get; init; }
}
