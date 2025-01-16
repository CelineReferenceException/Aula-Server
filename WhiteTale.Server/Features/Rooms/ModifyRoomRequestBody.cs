using System.Diagnostics.CodeAnalysis;

namespace WhiteTale.Server.Features.Rooms;

internal sealed class ModifyRoomRequestBody
{
	[MaybeNull]
	public String Name { get; init; }

	[MaybeNull]
	public String Description { get; init; }

	public Boolean IsEntrance { get; init; }
}
