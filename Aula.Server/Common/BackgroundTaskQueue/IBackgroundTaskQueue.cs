namespace Aula.Server.Common.BackgroundTaskQueue;

internal interface IBackgroundTaskQueue<T>
{
	ValueTask QueueBackgroundWorkItemAsync(Func<CancellationToken, ValueTask> workItem);
	ValueTask<Func<CancellationToken, ValueTask>> DequeueAsync(CancellationToken ct);
}
