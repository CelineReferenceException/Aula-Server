using FluentValidation;

#pragma warning disable CS8618
namespace Aula.Server.Domain.Messages;

internal sealed class Message : DefaultDomainEntity
{
	internal const MessageFlags StandardTypeAllowedFlags = MessageFlags.HideAuthor;

	internal const Int32 ContentMinimumLength = 1;
	internal const Int32 ContentMaximumLength = 2048;
	private static readonly MessageValidator s_validator = new();

	private Message()
	{
	}

	internal UInt64 Id { get; private init; }

	internal MessageType Type { get; private init; }

	internal MessageFlags Flags { get; private init; }

	internal MessageAuthorType AuthorType { get; private init; }

	internal UInt64? AuthorId { get; private init; }

	internal UInt64 RoomId { get; private init; }

	internal String? Content { get; private init; }

	internal MessageUserJoin? JoinData { get; private init; }

	internal MessageUserLeave? LeaveData { get; private init; }

	internal DateTime CreationTime { get; private init; }

	internal Boolean IsRemoved { get; private set; }

	internal static Message Create(
		UInt64 id,
		MessageType type,
		MessageFlags flags,
		MessageAuthorType authorType,
		UInt64? authorId,
		String? content,
		MessageUserJoin? joinData,
		MessageUserLeave? leaveData,
		UInt64 roomId)
	{
		var allowedFlags = type switch
		{
			MessageType.Standard => StandardTypeAllowedFlags,
			MessageType.UserJoin => (MessageFlags)0,
			MessageType.UserLeave => (MessageFlags)0,
			_ => throw new InvalidOperationException($"Invalid {nameof(MessageType)} value: '{type}')"),
		};

		flags = flags
			.GetFlags()
			.Where(flag => allowedFlags.HasFlag(flag))
			.Aggregate((x, y) => x | y);

		var message = new Message
		{
			Id = id,
			Type = type,
			Flags = flags,
			AuthorType = authorType,
			AuthorId = authorId,
			RoomId = roomId,
			Content = content,
			JoinData = joinData,
			LeaveData = leaveData,
			CreationTime = DateTime.UtcNow,
		};

		s_validator.ValidateAndThrow(message);

		message.AddEvent(new MessageCreatedEvent(message));

		return message;
	}

	internal void Remove()
	{
		IsRemoved = true;
		AddEvent(new MessageRemovedEvent(this));
	}
}
