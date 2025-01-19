using Microsoft.Extensions.DependencyInjection;
using WhiteTale.Server.Common.Persistence;
using WhiteTale.Server.Domain.Messages;

namespace WhiteTale.Server.IntegrationTests.Helpers;

internal static class MessageHelper
{
	internal static async Task<SeedMessageResult> SeedMessageAsync(this ApplicationInstance application, MessageSeed? messageSeed = null)
	{
		using var scope = application.Services.CreateScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

		messageSeed ??= MessageSeed.StandardTypeDefault;

		var message = Message.Create(messageSeed.Id, messageSeed.Type, messageSeed.Flags, messageSeed.AuthorId, messageSeed.Target,
			messageSeed.Content, messageSeed.TargetId);

		_ = dbContext.Messages.Add(message);
		_ = await dbContext.SaveChangesAsync();

		return new SeedMessageResult
		{
			Seed = messageSeed,
			Message = message
		};
	}
}
