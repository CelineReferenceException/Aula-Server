using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Retry;

namespace WhiteTale.Server.Features.Bans;

internal static class DependencyInjection
{
	internal static IServiceCollection AddBanFeatures(this IServiceCollection services)
	{
		_ = services.AddResiliencePipeline(BanResiliencePipelineNames.CleanUser, builder =>
		{
			_ = builder.AddRetry(new RetryStrategyOptions
			{
				Delay = TimeSpan.FromSeconds(1),
				ShouldHandle = new PredicateBuilder().Handle<DbUpdateConcurrencyException>(),
			});
		});

		return services;
	}
}
