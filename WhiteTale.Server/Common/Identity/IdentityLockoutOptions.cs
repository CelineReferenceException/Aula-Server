namespace WhiteTale.Server.Common.Identity;

internal sealed class IdentityLockoutOptions
{
	public Int32 LockoutMinutes { get; set; } = 15;

	public Int32 MaximumFailedAccessAttempts { get; set; } = 10;

	public Boolean AllowedForNewUsers { get; set; } = true;
}
