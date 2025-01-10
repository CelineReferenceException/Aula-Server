namespace WhiteTale.Server.IntegrationTests.Helpers;

/// <summary>
///     Used by <see cref="UserHelper.SeedUserAsync" /> to register new users into the application.
/// </summary>
internal sealed record UserSeed
{
	public String? DisplayName { get; init; }

	public required String UserName { get; init; }

	public required String Password { get; init; }

	public required String Email { get; init; }

	public Boolean EmailConfirmed { get; init; }
}
