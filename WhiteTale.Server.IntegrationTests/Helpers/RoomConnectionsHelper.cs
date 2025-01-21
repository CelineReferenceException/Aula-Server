using Microsoft.Extensions.DependencyInjection;
using WhiteTale.Server.Common.Persistence;
using WhiteTale.Server.Domain.Rooms;

namespace WhiteTale.Server.IntegrationTests.Helpers;

internal static class RoomConnectionHelper
{
	internal static async Task<SeedRoomConnectionResult> SeedRoomConnectionAsync(
		this ApplicationInstance application,
		RoomConnectionSeed? connectionSeed = null)
	{
		using var scope = application.Services.CreateScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

		connectionSeed ??= RoomConnectionSeed.Default;

		var connection = RoomConnection.Create(connectionSeed.Id, connectionSeed.SourceRoomId, connectionSeed.TargetRoomId);

		_ = dbContext.RoomConnections.Add(connection);
		_ = await dbContext.SaveChangesAsync();

		return new SeedRoomConnectionResult
		{
			Seed = connectionSeed,
			Connection = connection,
		};
	}
}
