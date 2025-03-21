﻿namespace Aula.Server.Core.Authentication;

internal static class AuthenticationSchemes
{
	private const String Prefix = nameof(AuthenticationSchemes);

	/// <summary>
	///     Clients can include a secret, sent in the HTTP 'Authorization' header using the "Bearer {secret}" format.
	/// </summary>
	internal const String BearerToken = $"{Prefix}.{nameof(BearerToken)}";
}
