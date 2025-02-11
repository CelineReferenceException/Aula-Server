using FluentValidation;

namespace WhiteTale.Server.Features.Rooms.Endpoints;

internal sealed class RemoveRoomConnectionRequestBodyValidator : AbstractValidator<RemoveRoomConnectionRequestBody>
{
	public RemoveRoomConnectionRequestBodyValidator()
	{
		_ = RuleFor(x => x.RoomId)
			.NotEmpty()
			.WithErrorCode($"{nameof(AddRoomConnectionRequestBody.RoomId)} is empty")
			.WithMessage($"{nameof(AddRoomConnectionRequestBody.RoomId)} cannot be empty.");
	}
}
