using FluentValidation;

namespace WhiteTale.Server.Features.Rooms.Endpoints;

internal sealed class RemoveConnectionRequestBodyValidator : AbstractValidator<RemoveRoomConnectionRequestBody>
{
	public RemoveConnectionRequestBodyValidator()
	{
		_ = RuleFor(x => x.RoomId)
			.NotEmpty()
			.WithErrorCode($"{nameof(AddRoomConnectionRequestBody.RoomId)} is empty")
			.WithMessage($"{nameof(AddRoomConnectionRequestBody.RoomId)} cannot be empty.");
	}
}
