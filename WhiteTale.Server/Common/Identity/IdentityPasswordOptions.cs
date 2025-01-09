namespace WhiteTale.Server.Common.Identity;

internal sealed class IdentityPasswordOptions
{
	public Boolean RequireUppercase { get; set; } = true;

	public Boolean RequireLowercase { get; set; } = true;

	public Boolean RequireDigit { get; set; } = true;

	public Boolean RequireNonAlphanumeric { get; set; } = true;

	public Int32 RequiredUniqueChars { get; set; }

	public Int32 RequiredLength { get; set; } = 8;
}
