#pragma warning disable CS8618
namespace WhiteTale.Server.Domain.Messages;

internal sealed class Message : DefaultDomainEntity
{
	internal const MessageFlags StandardTypeAllowedFlags = MessageFlags.HideAuthor;

	internal const Int32 ContentMaximumLength = 2048;

	private Message()
	{
	}

	internal UInt64 Id { get; private init; }

	internal MessageType Type { get; private init; }

	internal MessageFlags Flags { get; private init; }

	internal MessageAuthor AuthorType { get; private init; }

	internal UInt64? AuthorId { get; private init; }

	internal MessageTarget TargetType { get; private init; }

	internal UInt64 TargetId { get; private init; }

	internal String? Content { get; private init; }

	internal MessageUserJoin? JoinData { get; private init; }

	internal MessageUserLeave? LeaveData { get; private init; }

	internal DateTime CreationTime { get; private init; }

	internal Boolean IsRemoved { get; private set; }

	internal static Message Create(
		UInt64 id,
		MessageType type,
		MessageFlags flags,
		MessageAuthor authorType,
		UInt64? authorId,
		MessageTarget target,
		String? content,
		MessageUserJoin? joinData,
		MessageUserLeave? leaveData,
		UInt64 targetId)
	{
		var message = new Message
		{
			Id = id,
			Type = type,
			Flags = flags,
			AuthorType = authorType,
			AuthorId = authorId,
			TargetType = target,
			TargetId = targetId,
			Content = content,
			JoinData = joinData,
			LeaveData = leaveData,
			CreationTime = DateTime.UtcNow,
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
