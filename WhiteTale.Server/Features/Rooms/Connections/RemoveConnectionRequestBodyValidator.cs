using FluentValidation;

namespace WhiteTale.Server.Features.Rooms.Connections;

internal sealed class RemoveConnectionRequestBodyValidator : AbstractValidator<RemoveConnectionRequestBody>
{
	public RemoveConnectionRequestBodyValidator()
	{
		_ = RuleFor(x => x.TargetId)
			.NotEmpty()
			.WithErrorCode($"{nameof(AddConnectionRequestBody.TargetId)} is empty")
			.WithMessage($"{nameof(AddConnectionRequestBody.TargetId)} cannot be empty.");
	}
}
