using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Aula.Server.Common.BackgroundTaskQueue;

internal static class DependencyInjection
{
	internal static IServiceCollection AddBackgroundTaskQueueFor<T>(this IServiceCollection services)
	{
		services.TryAddSingleton<IBackgroundTaskQueue<T>, DefaultBackgroundTaskQueue<T>>();
		_ = services.AddHostedService<BackgroundTaskExecutor<T>>();

		return services;
	}
}
