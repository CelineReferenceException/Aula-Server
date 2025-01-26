namespace WhiteTale.Server.Features.Gateway.Events.Receive.Presences;

internal sealed record UpdatePresenceEventData
{
	/// <summary>
	///     The presence to use.
	/// </summary>
	public required PresenceOptions Presence { get; init; }
}
