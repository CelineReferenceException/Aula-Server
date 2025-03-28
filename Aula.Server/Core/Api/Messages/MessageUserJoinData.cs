﻿using Aula.Server.Domain;
using Aula.Server.Domain.Messages;

namespace Aula.Server.Core.Api.Messages;

/// <summary>
///     Holds data required by <see cref="MessageType.UserJoin" /> messages.
/// </summary>
internal sealed record MessageUserJoinData
{
	/// <summary>
	///     The ID of user who joined to this room.
	/// </summary>
	public required Snowflake UserId { get; init; }
}
