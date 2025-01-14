using System.Diagnostics.CodeAnalysis;

namespace WhiteTale.Server.Common.BackgroundTaskQueue;

internal sealed class QueueHostedService<T> : BackgroundService
{
	private readonly ILogger<QueueHostedService<T>> _logger;
	private readonly IBackgroundTaskQueue<T> _taskQueue;

	public QueueHostedService(IBackgroundTaskQueue<T> taskQueue, ILogger<QueueHostedService<T>> logger)
	{
		_taskQueue = taskQueue;
		_logger = logger;
	}

	[SuppressMessage("Design", "CA1031:Do not catch general exception types")]
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				var workItem = await _taskQueue.DequeueAsync(stoppingToken);
				await workItem(stoppingToken);
			}
			catch (OperationCanceledException)
			{
				// Prevent throwing if stoppingToken was signaled.
			}
			catch (Exception ex)
			{
				// Prevent stopping the service if the work failed.
				_logger.BackgroundWorkItemFailed(ex);
			}
		}
	}
}
