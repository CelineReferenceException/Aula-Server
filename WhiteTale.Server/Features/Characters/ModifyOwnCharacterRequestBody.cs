using System.Diagnostics.CodeAnalysis;

namespace WhiteTale.Server.Features.Characters;

internal sealed class ModifyOwnCharacterRequestBody
{
	private readonly String? _description;

	private readonly String? _displayName;

	/// <summary>
	///     The name of the character.
	/// </summary>
	[MaybeNull]
	public String DisplayName
	{
		get => _displayName;
		init => _displayName = value?.Trim();
	}

	/// <summary>
	///     The description of the character.
	/// </summary>
	[MaybeNull]
	public String Description
	{
		get => _description;
		init => _description = value?.Trim();
	}
}
