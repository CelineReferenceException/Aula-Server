namespace WhiteTale.Server.Common.Identity;

internal sealed class IdentityOptions
{
	internal const String SectionName = "Identity";

	public IdentitySignInOptions SignIn { get; set; } = new();

	public IdentityPasswordOptions Password { get; set; } = new();

	public IdentityLockoutOptions Lockout { get; set; } = new();
}
