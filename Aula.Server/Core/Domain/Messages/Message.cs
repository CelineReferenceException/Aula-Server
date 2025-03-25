using System.Diagnostics;
using FluentValidation.Results;

namespace Aula.Server.Core.Domain.Messages;

internal sealed class Message : DefaultDomainEntity
{
	internal const MessageFlags StandardTypeAllowedFlags = MessageFlags.HideAuthor;
	internal const MessageFlags UserJoinTypeAllowedFlags = 0;
	internal const MessageFlags UserLeaveTypeAllowedFlags = 0;
	internal const Int32 ContentMinimumLength = 1;
	internal const Int32 ContentMaximumLength = 2048;

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
	internal MessageUserJoin? JoinData { get; private init; }

	// Navigation property, values are set through reflection.
	internal MessageUserLeave? LeaveData { get; private init; }

	internal DateTime CreationDate { get; }

	internal Boolean IsRemoved { get; private set; }

	internal static Result<Message, ValidationFailure> Create(
		Snowflake id,
		MessageType type,
		MessageFlags flags,
		MessageAuthorType authorType,
		Snowflake? authorId,
		String? content,
		Snowflake roomId,
		MessageUserJoin? joinData,
		MessageUserLeave? leaveData)
	{
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

		var message = new Message(id, type, flags, authorType, authorId, roomId, content, DateTime.UtcNow, false)
		{
			JoinData = joinData,
			LeaveData = leaveData,
		};
		message.Events.Add(new MessageCreatedEvent(message));

		var validationResult = MessageValidator.Instance.Validate(message);
		return validationResult.IsValid
			? message
			: new ResultErrorValues<ValidationFailure>(validationResult.Errors);
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
