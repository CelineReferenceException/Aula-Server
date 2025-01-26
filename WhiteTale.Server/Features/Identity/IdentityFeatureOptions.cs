namespace WhiteTale.Server.Features.Identity;

internal sealed class IdentityFeatureOptions
{
	internal const String SectionName = "Identity";

	public Uri? ConfirmEmailRedirectUri { get; set; }

	public Uri? ResetPasswordRedirectUri { get; set; }

	public Permissions DefaultPermissions { get; set; }
}
