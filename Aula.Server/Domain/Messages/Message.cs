using System.Diagnostics;

namespace Aula.Server.Domain.Messages;

internal sealed class Message : DefaultDomainEntity
{
	internal const MessageFlags StandardTypeAllowedFlags = MessageFlags.HideAuthor;
	internal const MessageFlags UserJoinTypeAllowedFlags = 0;
	internal const MessageFlags UserLeaveTypeAllowedFlags = 0;
	internal const Int32 ContentMinimumLength = 1;
	internal const Int32 ContentMaximumLength = 2048;

	internal UInt64 Id { get; private init; }

	internal MessageType Type { get; private init; }

	internal MessageFlags Flags { get; private init; }

	internal MessageAuthorType AuthorType { get; private init; }

	internal UInt64? AuthorId { get; private init; }

	internal UInt64 RoomId { get; private init; }

	internal String? Content { get; private init; }

	internal MessageUserJoin? JoinData { get; private init; }

	internal MessageUserLeave? LeaveData { get; private init; }

	internal DateTime CreationDate { get; private init; }

	internal Boolean IsRemoved { get; private set; }

	internal Message(
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
		if (id is 0)
		{
			throw new ArgumentException($"{nameof(id)} cannot be 0.", nameof(id));
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
				.GetFlags()
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

		if (joinData is null &&
		    type is MessageType.UserJoin)
		{
			throw new ArgumentException($"{nameof(joinData)} cannot be null when {nameof(type)} is {MessageType.UserJoin}.",
				nameof(joinData));
		}

		if (leaveData is null &&
		    type is MessageType.UserLeave)
		{
			throw new ArgumentException($"{nameof(leaveData)} cannot be null when {nameof(type)} is {MessageType.UserLeave}.",
				nameof(leaveData));
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
		JoinData = joinData;
		LeaveData = leaveData;
		CreationDate = DateTime.UtcNow;

		AddEvent(new MessageCreatedEvent(this));
	}

	internal void Remove()
	{
		IsRemoved = true;
		AddEvent(new MessageRemovedEvent(this));
	}
}
