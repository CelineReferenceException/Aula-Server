﻿namespace Aula.Server.Core.Features;

/// <summary>
///     Represents a user within the application.
/// </summary>
internal sealed record UserData
{
	/// <summary>
	///     The ID of the user.
	/// </summary>
	public required Snowflake Id { get; init; }

	/// <summary>
	///     The name of the user.
	/// </summary>
	public required String DisplayName { get; init; }

	/// <summary>
	///     The description of the user.
	/// </summary>
	public required String Description { get; init; }

	/// <summary>
	///     The type of entity who owns the user.
	/// </summary>
	public required UserType Type { get; init; }

	/// <summary>
	///     The presence of the user.
	/// </summary>
	public required Presence Presence { get; init; }

	/// <summary>
	///     The permissions of the user.
	/// </summary>
	public required Permissions Permissions { get; init; }

	/// <summary>
	///     The ID of the current room the user resides in.
	/// </summary>
	public required Snowflake? CurrentRoomId { get; init; }
}
