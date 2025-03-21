using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Aula.Server.Core.BackgroundTaskQueue;

internal static class DependencyInjection
{
	internal static IServiceCollection AddBackgroundTaskQueueFor<T>(this IServiceCollection services)
	{
		services.TryAddSingleton<IBackgroundTaskQueue<T>, DefaultBackgroundTaskQueue<T>>();
		_ = services.AddHostedService<QueueHostedService<T>>();

		return services;
	}
}
