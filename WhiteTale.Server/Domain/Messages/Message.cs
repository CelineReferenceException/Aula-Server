#pragma warning disable CS8618
namespace WhiteTale.Server.Domain.Messages;

internal sealed class Message : DomainEntity
{
	internal const Int32 ContentMaximumLength = 2048;

	private Message()
	{
	}

	internal UInt64 Id { get; private init; }

	internal MessageFlags Flags { get; private init; }

	internal UInt64 AuthorId { get; private init; }

	internal MessageTarget Target { get; private init; }

	internal UInt64? RoomId { get; private init; }

	internal String Content { get; private init; }

	internal DateTime CreationTime { get; private init; }

	internal Boolean IsRemoved { get; private set; }

	internal static Message Create(
		UInt64 id,
		MessageFlags flags,
		UInt64 authorId,
		MessageTarget target,
		String content,
		UInt64? roomId = null)
	{
		var message = new Message
		{
			Id = id,
			Flags = flags,
			AuthorId = authorId,
			Target = target,
			RoomId = roomId,
			Content = content,
			CreationTime = DateTime.UtcNow
		};

		message.AddEvent(new MessageCreatedEvent(message));

		return message;
	}

	internal void Remove()
	{
		IsRemoved = true;
		AddEvent(new MessageRemovedEvent(this));
	}
}
