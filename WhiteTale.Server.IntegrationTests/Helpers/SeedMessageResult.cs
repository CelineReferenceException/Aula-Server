using WhiteTale.Server.Domain.Messages;

namespace WhiteTale.Server.IntegrationTests.Helpers;

internal sealed class SeedMessageResult
{
	internal required MessageSeed Seed { get; init; }

	internal required Message Message { get; init; }
}
