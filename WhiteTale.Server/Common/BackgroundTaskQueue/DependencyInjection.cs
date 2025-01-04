using Microsoft.Extensions.DependencyInjection.Extensions;

namespace WhiteTale.Server.Common.BackgroundTaskQueue;

internal static class DependencyInjection
{
	internal static IServiceCollection AddBackgroundTaskQueueFor<TOwner>(this IServiceCollection services)
	{
		services.TryAddSingleton<IBackgroundTaskQueue<TOwner>, DefaultBackgroundTaskQueue<TOwner>>();
		_ = services.AddHostedService<QueueHostedService<TOwner>>();

		return services;
	}
}
