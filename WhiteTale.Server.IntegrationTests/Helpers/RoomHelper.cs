using Microsoft.Extensions.DependencyInjection;
using WhiteTale.Server.Common;
using WhiteTale.Server.Common.Persistence;
using WhiteTale.Server.Domain.Rooms;

namespace WhiteTale.Server.IntegrationTests.Helpers;

internal static class RoomHelper
{


	internal static async Task<SeedRoomResult> SeedRoomAsync(this ApplicationInstance application, RoomSeed? roomSeed = null)
	{
		using var scope = application.Services.CreateScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

		roomSeed ??= RoomSeed.Default;

		var room = Room.Create(roomSeed.Id, roomSeed.Name, roomSeed.Description, roomSeed.IsEntrance);

		_ = dbContext.Rooms.Add(room);
		_ = await dbContext.SaveChangesAsync();

		return new SeedRoomResult
		{
			Seed = roomSeed,
			Room = room
		};
	}
}
