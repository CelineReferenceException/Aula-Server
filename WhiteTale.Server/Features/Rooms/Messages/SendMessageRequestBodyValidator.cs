using FluentValidation;

namespace WhiteTale.Server.Features.Rooms.Messages;

internal sealed class SendMessageRequestBodyValidator : AbstractValidator<SendMessageRequestBody>
{
	public SendMessageRequestBodyValidator()
	{
		_ = RuleFor(x => x.Type).IsInEnum();
		_ = RuleFor(x => x.Flags).IsInEnum();
		_ = RuleFor(x => x.Target).IsInEnum();

		_ = When(x => x.Type is MessageType.Standard, () =>
		{
			_ = RuleFor(x => x.Content)
				.NotEmpty()
				.MaximumLength(Message.ContentMaximumLength)
				.WithErrorCode($"{nameof(Message.Content)} is too long")
				.WithMessage($"{nameof(Message.Content)} length must be at most {Message.ContentMaximumLength}");
		});
	}
}
