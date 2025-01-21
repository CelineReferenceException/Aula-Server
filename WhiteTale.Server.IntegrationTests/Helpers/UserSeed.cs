using WhiteTale.Server.Domain.Users;

namespace WhiteTale.Server.IntegrationTests.Helpers;

/// <summary>
///     Used by <see cref="UserHelper.SeedUserAsync" /> to register new users into the application.
/// </summary>
internal sealed record UserSeed
{
	internal static UserSeed Default { get; } = new()
	{
		Id = 1,
		DisplayName = "TestUser",
		UserName = "test_user",
		Password = "TestPassword1!",
		Email = "test_address@example.com",
		EmailConfirmed = true,
		CurrentRoomId = null,
	};

	public required UInt64 Id { get; init; }

	public String? DisplayName { get; init; }

	public required String UserName { get; init; }

	public required String Password { get; init; }

	public required String Email { get; init; }

	public Boolean EmailConfirmed { get; init; }

	public Permissions Permissions { get; init; }

	public UInt64? CurrentRoomId { get; init; }
}
