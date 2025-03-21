﻿namespace Aula.Server.Core.Features.Users;

/// <summary>
///     A request from a client to update the presence of its user.
/// </summary>
internal sealed record UpdatePresenceEventData
{
	/// <summary>
	///     The presence to use.
	/// </summary>
	public required PresenceOptions Presence { get; init; }
}
