using System.Threading.Channels;

namespace WhiteTale.Server.Common.BackgroundTaskQueue;

internal sealed class DefaultBackgroundTaskQueue<TOwner> : IBackgroundTaskQueue<TOwner>
{
	private readonly Channel<Func<CancellationToken, ValueTask>> _taskQueue = Channel.CreateUnbounded<Func<CancellationToken, ValueTask>>();

	public async ValueTask QueueBackgroundWorkItemAsync(Func<CancellationToken, ValueTask> workItem)
	{
		ArgumentNullException.ThrowIfNull(workItem, nameof(workItem));

		_ = await _taskQueue.Writer.WaitToWriteAsync();
		await _taskQueue.Writer.WriteAsync(workItem);
	}

	public async ValueTask<Func<CancellationToken, ValueTask>> DequeueAsync(CancellationToken ct)
	{
		_ = await _taskQueue.Reader.WaitToReadAsync(ct);
		var workItem = await _taskQueue.Reader.ReadAsync(ct);

		return workItem;
	}
}
