namespace Aula.Server.Common.Identity;

internal static class TokenProviderExtensions
{
	internal static String CreateToken(this TokenProvider tokenProvider, User user)
	{
		var id = user.Id.ToString();
		var stamp = user.SecurityStamp ?? throw new ArgumentException("The user security stamp cannot be null.");
		return tokenProvider.CreateToken(id, stamp);
	}
}
