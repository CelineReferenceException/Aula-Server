namespace WhiteTale.Server.Features.Gateway.Events.Receive.Presences;

internal sealed class UpdatePresenceEventData
{
	/// <summary>
	///     The presence to use.
	/// </summary>
	public required PresenceOptions Presence { get; init; }
}
