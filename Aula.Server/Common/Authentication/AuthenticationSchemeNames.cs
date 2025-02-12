namespace Aula.Server.Common.Authentication;

internal static class AuthenticationSchemeNames
{
	private const String Prefix = nameof(AuthenticationSchemeNames);
	internal const String BearerToken = $"{Prefix}.{nameof(BearerToken)}";
}
