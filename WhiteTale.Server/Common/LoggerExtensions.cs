namespace WhiteTale.Server.Common;

internal static partial class LoggerExtensions
{
	[LoggerMessage(LogLevel.Information, "{message}")]
	internal static partial void Inform(this ILogger logger, String message);

	[LoggerMessage(LogLevel.Trace, "{message}")]
	internal static partial void Trace(this ILogger logger, String message);

	[LoggerMessage(LogLevel.Error, "{tableName} with id '{id}' has been modified since it was loaded into memory.")]
	internal static partial void LogDbUpdateConcurrencyProblem(this ILogger logger, String tableName, Object id, Exception ex);
}
