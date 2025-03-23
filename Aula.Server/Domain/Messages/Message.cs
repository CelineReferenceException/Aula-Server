using System.Diagnostics;

namespace Aula.Server.Domain.Messages;

internal sealed class Message : DefaultDomainEntity
{
	internal const MessageFlags StandardTypeAllowedFlags = MessageFlags.HideAuthor;
	internal const MessageFlags UserJoinTypeAllowedFlags = 0;
	internal const MessageFlags UserLeaveTypeAllowedFlags = 0;
	internal const Int32 ContentMinimumLength = 1;
	internal const Int32 ContentMaximumLength = 2048;

	private static readonly ResultProblem s_unknownMessageType =
		new("Unknown message type", "An unknown message type was provided.");

	private static readonly ResultProblem s_unknownMessageFlags =
		new("Unknown message flags", "One or more of the permissions provided are not valid.");

	private static readonly ResultProblem s_unknownAuthorType =
		new("Unknown author type", "An unknown author type was provided.");

	private static readonly ResultProblem s_contentTooShort =
		new("Content is too short", $"Content length must be at least {ContentMinimumLength}.");

	private static readonly ResultProblem s_contentTooLong =
		new("Content is too long", $"Content length must be at most {ContentMaximumLength}.");

	private static readonly ResultProblem s_userAuthorWithNullId =
		new("Invalid author", "A user author cannot have a null id.");

	private Message(
		Snowflake id,
		MessageType type,
		MessageFlags flags,
		MessageAuthorType authorType,
		Snowflake? authorId,
		Snowflake roomId,
		String? content,
		DateTime creationDate,
		Boolean isRemoved)
	{
		Id = id;
		Type = type;
		Flags = flags;
		AuthorType = authorType;
		AuthorId = authorId;
		RoomId = roomId;
		Content = content;
		CreationDate = creationDate;
		IsRemoved = isRemoved;
	}

	internal Snowflake Id { get; }

	internal MessageType Type { get; }

	internal MessageFlags Flags { get; }

	internal MessageAuthorType AuthorType { get; }

	internal Snowflake? AuthorId { get; }

	internal Snowflake RoomId { get; }

	internal String? Content { get; }

	// Navigation property, values are set through reflection.
	internal MessageUserJoin? JoinData { get; init; }

	// Navigation property, values are set through reflection.
	internal MessageUserLeave? LeaveData { get; init; }

	internal DateTime CreationDate { get; }

	internal Boolean IsRemoved { get; private set; }

	internal static Result<Message> Create(
		Snowflake id,
		MessageType type,
		MessageFlags flags,
		MessageAuthorType authorType,
		Snowflake? authorId,
		String? content,
		Snowflake roomId)
	{
		var problems = new Items<ResultProblem>();

		if (!Enum.IsDefined(type))
		{
			problems.Add(s_unknownMessageType);
		}

		if (!Enum.IsDefined(flags))
		{
			problems.Add(s_unknownMessageFlags);
		}

		if (!Enum.IsDefined(authorType))
		{
			problems.Add(s_unknownAuthorType);
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
			problems.Add(s_userAuthorWithNullId);
		}

		if (content is not null)
		{
			switch (content.Length)
			{
				case < ContentMinimumLength: problems.Add(s_contentTooShort); break;
				case > ContentMaximumLength: problems.Add(s_contentTooLong); break;
				default: break;
			}
		}

		if (problems.Count > 0)
		{
			return new ResultProblemValues(problems);
		}

		var message = new Message(id, type, flags, authorType, authorId, roomId, content, DateTime.UtcNow, false);
		message.Events.Add(new MessageCreatedEvent(message));
		return message;
	}

	internal void Remove()
	{
		if (IsRemoved)
		{
			return;
		}

		IsRemoved = true;
		Events.Add(new MessageRemovedEvent(this));
	}
}
