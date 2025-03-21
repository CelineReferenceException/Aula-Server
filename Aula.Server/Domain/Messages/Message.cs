using System.Diagnostics;

namespace Aula.Server.Domain.Messages;

internal sealed class Message : DefaultDomainEntity
{
	internal const MessageFlags StandardTypeAllowedFlags = MessageFlags.HideAuthor;
	internal const MessageFlags UserJoinTypeAllowedFlags = 0;
	internal const MessageFlags UserLeaveTypeAllowedFlags = 0;
	internal const Int32 ContentMinimumLength = 1;
	internal const Int32 ContentMaximumLength = 2048;

	internal UInt64 Id { get; }

	internal MessageType Type { get; }

	internal MessageFlags Flags { get; }

	internal MessageAuthorType AuthorType { get; }

	internal UInt64? AuthorId { get; }

	internal UInt64 RoomId { get; }

	internal String? Content { get; }

	// Navigation property, values are set through reflection.
	internal MessageUserJoin? JoinData { get; init; }

	// Navigation property, values are set through reflection.
	internal MessageUserLeave? LeaveData { get; init; }

	internal DateTime CreationDate { get; }

	internal Boolean IsRemoved { get; private set; }

	internal Message(
		UInt64 id,
		MessageType type,
		MessageFlags flags,
		MessageAuthorType authorType,
		UInt64? authorId,
		String? content,
		UInt64 roomId)
	{
		if (id is 0)
		{
			throw new ArgumentException($"{nameof(id)} cannot be 0.", nameof(id));
		}

		if (!Enum.IsDefined(type))
		{
			throw new ArgumentOutOfRangeException(nameof(type));
		}

		if (!Enum.IsDefined(flags))
		{
			throw new ArgumentOutOfRangeException(nameof(type));
		}

		if (!Enum.IsDefined(authorType))
		{
			throw new ArgumentOutOfRangeException(nameof(type));
		}

		if (flags > 0)
		{
			var allowedFlags = type switch
			{
				MessageType.Standard => StandardTypeAllowedFlags,
				MessageType.UserJoin => UserJoinTypeAllowedFlags,
				MessageType.UserLeave => UserLeaveTypeAllowedFlags,
				_ => throw new UnreachableException($"No case defined for {nameof(MessageType)}.{type}"),
			};

			flags = flags
				.GetDefinedFlags()
				.Where(flag => allowedFlags.HasFlag(flag))
				.Aggregate((x, y) => x | y);
		}

		if (authorType is MessageAuthorType.User &&
		    authorId is null)
		{
			throw new ArgumentException($"{nameof(authorId)} cannot be null when {nameof(authorType)} is {MessageAuthorType.User}.",
				nameof(authorId));
		}

		if (authorId is 0)
		{
			throw new ArgumentException($"{nameof(id)} cannot be 0.", nameof(id));
		}

		if (content is not null)
		{
			switch (content.Length)
			{
				case < ContentMinimumLength:
					throw new ArgumentOutOfRangeException(nameof(content),
						$"{nameof(content)} length must be at least {ContentMinimumLength}.");
				case > ContentMaximumLength:
					throw new ArgumentOutOfRangeException(nameof(content),
						$"{nameof(content)} length must be at most ${ContentMaximumLength}.");
				default: break;
			}
		}

		if (roomId is 0)
		{
			throw new ArgumentException($"{nameof(roomId)} cannot be 0.", nameof(roomId));
		}

		Id = id;
		Type = type;
		Flags = flags;
		AuthorType = authorType;
		AuthorId = authorId;
		RoomId = roomId;
		Content = content;
		CreationDate = DateTime.UtcNow;

		AddEvent(new MessageCreatedEvent(this));
	}

	internal void Remove()
	{
		IsRemoved = true;
		AddEvent(new MessageRemovedEvent(this));
	}
}
