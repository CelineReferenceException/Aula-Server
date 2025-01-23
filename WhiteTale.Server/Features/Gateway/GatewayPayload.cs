namespace WhiteTale.Server.Features.Gateway;

internal sealed class GatewayPayload : GatewayPayload<String>;

internal class GatewayPayload<TData>
{
	public required OperationType Operation { get; init; }

	public EventType? Event { get; init; }

	public TData? Data { get; init; }
}
