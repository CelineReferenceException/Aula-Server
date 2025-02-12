using FluentValidation;

namespace WhiteTale.Server.Features.Messages.Endpoints;

internal sealed class SendMessageRequestBodyValidator : AbstractValidator<SendMessageRequestBody>
{
	public SendMessageRequestBodyValidator()
	{
		_ = RuleFor(x => x.Type).IsInEnum();
		_ = RuleFor(x => x.Flags).IsInEnum();

		_ = When(x => x.Type is MessageType.Standard, () =>
		{
			_ = RuleFor(x => x.Content)
				.NotNull()
				.WithErrorCode($"{nameof(SendMessageRequestBody.Content)} is null")
				.WithMessage($"{nameof(SendMessageRequestBody.Content)} cannot be null.");

			_ = RuleFor(x => x.Content)
				.MaximumLength(Message.ContentMinimumLength)
				.WithErrorCode($"{nameof(SendMessageRequestBody.Content)} is too short")
				.WithMessage($"{nameof(SendMessageRequestBody.Content)} length must be at least {Message.ContentMinimumLength}");

			_ = RuleFor(x => x.Content)
				.MaximumLength(Message.ContentMaximumLength)
				.WithErrorCode($"{nameof(SendMessageRequestBody.Content)} is too long")
				.WithMessage($"{nameof(SendMessageRequestBody.Content)} length must be at most {Message.ContentMaximumLength}");
		});
	}
}
