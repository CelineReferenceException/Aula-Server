using System.Threading.Channels;

namespace WhiteTale.Server.Common.BackgroundTaskQueue;

internal sealed class DefaultBackgroundTaskQueue<TOwner> : IBackgroundTaskQueue<TOwner>
{
	private const Int32 DefaultCapacity = 64;
	private readonly ILogger<DefaultBackgroundTaskQueue<TOwner>> _logger;
	private readonly Channel<Func<CancellationToken, ValueTask>> _taskQueue;

	public DefaultBackgroundTaskQueue(ILogger<DefaultBackgroundTaskQueue<TOwner>> logger)
	{
		_logger = logger;

		var queueOptions = new BoundedChannelOptions(DefaultCapacity)
		{
			FullMode = BoundedChannelFullMode.Wait,
		};
		_taskQueue = Channel.CreateBounded<Func<CancellationToken, ValueTask>>(queueOptions);
	}


	public async ValueTask QueueBackgroundWorkItemAsync(Func<CancellationToken, ValueTask> workItem)
	{
		ArgumentNullException.ThrowIfNull(workItem, nameof(workItem));

		_ = await _taskQueue.Writer.WaitToWriteAsync();
		_logger.Trace("Queueing background work item");
		await _taskQueue.Writer.WriteAsync(workItem);
	}

	public async ValueTask<Func<CancellationToken, ValueTask>> DequeueAsync(CancellationToken ct)
	{
		_ = await _taskQueue.Reader.WaitToReadAsync(ct);
		_logger.Trace("Dequeueing background work item");
		var workItem = await _taskQueue.Reader.ReadAsync(ct);

		return workItem;
	}
}
