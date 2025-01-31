using FluentValidation;

namespace WhiteTale.Server.Features.RoomConnections.Endpoints;

internal sealed class AddConnectionRequestBodyValidator : AbstractValidator<AddConnectionRequestBody>
{
	public AddConnectionRequestBodyValidator()
	{
		_ = RuleFor(x => x.RoomId)
			.NotEmpty()
			.WithErrorCode($"{nameof(AddConnectionRequestBody.RoomId)} is empty")
			.WithMessage($"{nameof(AddConnectionRequestBody.RoomId)} cannot be empty.");
	}
}
