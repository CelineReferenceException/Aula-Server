using FluentValidation;

namespace Aula.Server.Domain.Messages;

internal sealed class MessageValidator : AbstractValidator<Message>
{
	public MessageValidator()
	{
		var messageTypes = Enum.GetValues<MessageType>();
		var messageAuthorTypes = Enum.GetValues<MessageAuthorType>();

		_ = RuleFor(x => x.Type)
			.IsInEnum()
			.WithErrorCode(nameof(Message.Type))
			.WithMessage($"Unknown value. Known values are {String.Join(", ", messageTypes.Cast<Int32>())}.");

		_ = RuleFor(x => x.AuthorType)
			.IsInEnum()
			.WithErrorCode(nameof(Message.AuthorType))
			.WithMessage($"Unknown value. Known values are {String.Join(", ", messageAuthorTypes.Cast<Int32>())}.");

		_ = When(x => x.AuthorType is MessageAuthorType.User, () =>
		{
			_ = RuleFor(x => x.AuthorId)
				.NotNull()
				.WithErrorCode(nameof(Message.AuthorId))
				.WithMessage($"Required when {nameof(Message.AuthorType)} is {(Int32)MessageAuthorType.User}.");
		});
		_ = When(x => x.AuthorType is MessageAuthorType.System, () =>
		{
			_ = RuleFor(x => x.AuthorId)
				.Null()
				.WithErrorCode(nameof(Message.AuthorId))
				.WithMessage($"Must be null when {nameof(Message.AuthorType)} is {(Int32)MessageAuthorType.System}.");
		});

		_ = When(x => x.Type is MessageType.Standard, () =>
		{
			_ = RuleFor(x => x.Content)
				.NotNull()
				.WithErrorCode(nameof(Message.Content))
				.WithMessage($"Required when {nameof(Message.Type)} is {(Int32)MessageType.Standard}.");

			_ = RuleFor(x => x.Content)
				.MinimumLength(Message.ContentMinimumLength)
				.WithErrorCode(nameof(Message.Content))
				.WithMessage($"Length must be at least {Message.ContentMinimumLength}.");

			_ = RuleFor(x => x.Content)
				.MaximumLength(Message.ContentMaximumLength)
				.WithErrorCode(nameof(Message.Content))
				.WithMessage($"Length must be at most {Message.ContentMaximumLength}.");

			_ = RuleFor(x => x.JoinData)
				.Null()
				.WithErrorCode(nameof(Message.JoinData))
				.WithMessage($"Must be null when {nameof(Message.Type)} is {(Int32)MessageType.Standard}.");

			_ = RuleFor(x => x.LeaveData)
				.Null()
				.WithErrorCode(nameof(Message.LeaveData))
				.WithMessage($"Must be null when {nameof(Message.Type)} is {(Int32)MessageType.Standard}.");
		});

		_ = When(x => x.Type is MessageType.UserJoin, () =>
		{
			_ = RuleFor(x => x.Content)
				.Null()
				.WithErrorCode(nameof(Message.Content))
				.WithMessage($"Must be null when {nameof(Message.Type)} is {(Int32)MessageType.UserJoin}.");

			_ = RuleFor(x => x.JoinData)
				.NotNull()
				.WithErrorCode(nameof(Message.JoinData))
				.WithMessage($"Required when {nameof(Message.Type)} is {(Int32)MessageType.UserJoin}.");

			_ = RuleFor(x => x.LeaveData)
				.Null()
				.WithErrorCode(nameof(Message.LeaveData))
				.WithMessage($"Must be null when {nameof(Message.Type)} is {(Int32)MessageType.UserJoin}.");
		});

		_ = When(x => x.Type is MessageType.UserLeave, () =>
		{
			_ = RuleFor(x => x.Content)
				.Null()
				.WithErrorCode(nameof(Message.Content))
				.WithMessage($"Must be null when {nameof(Message.Type)} is {(Int32)MessageType.UserLeave}.");

			_ = RuleFor(x => x.JoinData)
				.Null()
				.WithErrorCode(nameof(Message.JoinData))
				.WithMessage($"Must be null when {nameof(Message.Type)} is {(Int32)MessageType.UserLeave}.");

			_ = RuleFor(x => x.LeaveData)
				.NotNull()
				.WithErrorCode(nameof(Message.LeaveData))
				.WithMessage($"Required when {nameof(Message.Type)} is {(Int32)MessageType.UserLeave}.");
		});
	}
}
