namespace WhiteTale.Server.Features.Gateway.Events.Send.Hello;

internal sealed record HelloEventPayloadData
{
	/// <summary>
	///     The ID of the session.
	/// </summary>
	public required String SessionId { get; init; }
}
