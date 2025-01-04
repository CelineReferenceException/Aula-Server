namespace WhiteTale.Server.Common.BackgroundTaskQueue;

internal static partial class LoggerExtensions
{
	[LoggerMessage(LogLevel.Error, "Background task work item failed")]
	internal static partial void BackgroundWorkItemFailed(this ILogger logger, Exception ex);
}
