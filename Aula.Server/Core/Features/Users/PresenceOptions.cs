﻿namespace Aula.Server.Core.Features.Users;

/// <summary>
///     Define available presences statuses to select for a user.
/// </summary>
internal enum PresenceOptions
{
	/// <summary>
	///     Show the user as offline.
	/// </summary>
	Invisible = 0,

	/// <summary>
	///     Show the user as online.
	/// </summary>
	Online = 1,
}
