using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Retry;

namespace WhiteTale.Server.Common.Resilience;

internal static class DependencyInjection
{
	internal static IServiceCollection AddResilience(this IServiceCollection services)
	{
		_ = services.AddResiliencePipeline(ResiliencePipelineNames.RetryOnDbConcurrencyProblem, builder =>
		{
			_ = builder
				.AddRetry(new RetryStrategyOptions
				{
					MaxRetryAttempts = Int32.MaxValue,
					ShouldHandle = new PredicateBuilder().Handle<DbUpdateConcurrencyException>(),
				});
		});

		return services;
	}
}
