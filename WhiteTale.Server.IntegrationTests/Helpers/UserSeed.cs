using WhiteTale.Server.Domain.Users;

namespace WhiteTale.Server.IntegrationTests.Helpers;

/// <summary>
///     Used by <see cref="UserHelper.SeedUserAsync" /> to register new users into the application.
/// </summary>
internal sealed record UserSeed
{
	public required UInt64 Id { get; init; }

	public String? DisplayName { get; init; }

	public required String UserName { get; init; }

	public required String Password { get; init; }

	public required String Email { get; init; }

	public Boolean EmailConfirmed { get; init; }

	public Permissions Permissions { get; init; }
}
