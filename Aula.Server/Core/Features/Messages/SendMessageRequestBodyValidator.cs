using FluentValidation;

namespace Aula.Server.Core.Features.Messages;

internal sealed class SendMessageRequestBodyValidator : AbstractValidator<SendMessageRequestBody>
{
	public SendMessageRequestBodyValidator()
	{
		var allowedMessageTypes = new[] { MessageType.Standard, };
		var messageFlags = Enum.GetValues<MessageFlags>();

		_ = RuleFor(x => x.Type)
			.IsInEnum()
			.Must(v => allowedMessageTypes.Any(allowedType => v == allowedType))
			.WithErrorCode(nameof(SendMessageRequestBody.Type).ToCamel())
			.WithMessage($"Invalid value. Valid values are: {String.Join(", ", allowedMessageTypes.Cast<Int32>())}.");

		_ = RuleFor(x => x.Flags)
			.IsInEnum()
			.WithErrorCode(nameof(SendMessageRequestBody.Flags).ToCamel())
			.WithMessage("Invalid value. " +
			             $"Valid values are combinations of the following flags: {String.Join(", ", messageFlags.Cast<Int32>())}.");

		_ = When(x => x.Type is MessageType.Standard, () =>
		{
			_ = RuleFor(x => x.Content)
				.NotNull()
				.WithErrorCode(nameof(SendMessageRequestBody.Content).ToCamel())
				.WithMessage($"Required when {nameof(SendMessageRequestBody.Content).ToCamel()} is {(Int32)MessageType.Standard}.");

			_ = RuleFor(x => x.Content)
				.MinimumLength(Message.ContentMinimumLength)
				.WithErrorCode(nameof(SendMessageRequestBody.Content).ToCamel())
				.WithMessage($"Length must be at least {Message.ContentMinimumLength}.");

			_ = RuleFor(x => x.Content)
				.MaximumLength(Message.ContentMaximumLength)
				.WithErrorCode(nameof(SendMessageRequestBody.Content).ToCamel())
				.WithMessage($"Length must be at most {Message.ContentMaximumLength}.");
		});
	}
}
