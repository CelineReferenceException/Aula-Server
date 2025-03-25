using FluentValidation;

namespace Aula.Server.Domain.Messages;

internal sealed class MessageValidator : AbstractValidator<Message>
{
	public MessageValidator()
	{
		_ = RuleFor(x => x.Type).IsInEnum();
		_ = RuleFor(x => x.AuthorType).IsInEnum();

		_ = When(x => x.AuthorType is MessageAuthorType.User, () =>
		{
			_ = RuleFor(x => x.AuthorId).NotNull();
		});

		_ = When(x => x.AuthorType is MessageAuthorType.System, () =>
		{
			_ = RuleFor(x => x.AuthorId).Null();
		});

		_ = When(x => x.Type is MessageType.Standard, () =>
		{
			_ = RuleFor(x => x.Content).NotNull();
			_ = RuleFor(x => x.Content).MinimumLength(Message.ContentMinimumLength);
			_ = RuleFor(x => x.Content).MaximumLength(Message.ContentMaximumLength);
			_ = RuleFor(x => x.JoinData).Null();
			_ = RuleFor(x => x.LeaveData).Null();
		});

		_ = When(x => x.Type is MessageType.UserJoin, () =>
		{
			_ = RuleFor(x => x.Content).Null();
			_ = RuleFor(x => x.JoinData).NotNull();
			_ = RuleFor(x => x.LeaveData).Null();
		});

		_ = When(x => x.Type is MessageType.UserLeave, () =>
		{
			_ = RuleFor(x => x.Content).Null();
			_ = RuleFor(x => x.JoinData).Null();
			_ = RuleFor(x => x.LeaveData).NotNull();
		});
	}

	internal static MessageValidator Instance { get; } = new();
}
