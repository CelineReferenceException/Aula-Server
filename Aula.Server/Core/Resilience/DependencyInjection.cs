using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Retry;

namespace Aula.Server.Core.Resilience;

internal static class DependencyInjection
{
	internal static IServiceCollection AddResilience(this IServiceCollection services)
	{
		_ = services.AddResiliencePipeline(ResiliencePipelines.RetryOnDbConcurrencyProblem, builder =>
		{
			_ = builder
				.AddRetry(new RetryStrategyOptions
				{
					MaxRetryAttempts = Int32.MaxValue - 1,
					ShouldHandle = new PredicateBuilder().Handle<DbUpdateConcurrencyException>(),
				});
		});

		return services;
	}
}
