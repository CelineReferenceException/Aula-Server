namespace WhiteTale.Server.Features.Identity;

internal sealed class IdentityFeatureOptions
{
	internal const String SectionName = "Identity";

	public Uri? ResetPasswordRedirectUri { get; set; }
}
