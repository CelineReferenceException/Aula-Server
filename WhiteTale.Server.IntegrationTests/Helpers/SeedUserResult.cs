using WhiteTale.Server.Domain.Users;

namespace WhiteTale.Server.IntegrationTests.Helpers;

internal sealed record SeedUserResult
{
	internal required UserSeed Seed { get; init; }

	internal required User User { get; init; }
}
