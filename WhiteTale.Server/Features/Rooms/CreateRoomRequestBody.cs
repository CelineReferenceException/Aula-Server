using System.Diagnostics.CodeAnalysis;

namespace WhiteTale.Server.Features.Rooms;

internal sealed class CreateRoomRequestBody
{
	public required String Name { get; init; }

	[MaybeNull]
	public String Description { get; init; }

	public Boolean IsEntrance { get; init; }
}
