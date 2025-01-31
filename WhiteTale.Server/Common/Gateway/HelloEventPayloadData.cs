namespace WhiteTale.Server.Common.Gateway;

/// <summary>
///     Sent at the start of a gateway connection (not when reconnecting). Contains useful data for future use.
/// </summary>
internal sealed record HelloEventPayloadData
{
	/// <summary>
	///     The ID of the session.
	/// </summary>
	public required String SessionId { get; init; }
}
