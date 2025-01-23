using FluentValidation;

namespace WhiteTale.Server.Features.Users.CurrentRoom;

internal sealed class SetCurrentRoomRequestBodyValidator : AbstractValidator<SetCurrentRoomRequestBody>
{
	public SetCurrentRoomRequestBodyValidator()
	{
		_ = RuleFor(x => x.RoomId)
			.NotEmpty()
			.WithErrorCode($"{nameof(SetCurrentRoomRequestBody.RoomId)} is empty")
			.WithMessage($"{nameof(SetCurrentRoomRequestBody.RoomId)} cannot be empty.");
	}
}
