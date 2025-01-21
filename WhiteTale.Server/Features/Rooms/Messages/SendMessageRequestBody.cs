namespace WhiteTale.Server.Features.Rooms.Messages;

internal sealed class SendMessageRequestBody
{
	public required MessageType Type { get; init; }

	public MessageFlags? Flags { get; init; }

	public MessageTarget? Target { get; init; }

	public String? Content { get; init; }
}
