using FluentValidation;

namespace WhiteTale.Server.Features.Rooms.Connections;

internal sealed class AddConnectionRequestBodyValidator : AbstractValidator<AddConnectionRequestBody>
{
	public AddConnectionRequestBodyValidator()
	{
		_ = RuleFor(x => x.TargetId)
			.NotEmpty()
			.WithErrorCode($"{nameof(AddConnectionRequestBody.TargetId)} is empty")
			.WithMessage($"{nameof(AddConnectionRequestBody.TargetId)} cannot be empty.");
	}
}
