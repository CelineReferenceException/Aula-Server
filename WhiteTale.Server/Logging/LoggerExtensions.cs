using Microsoft.Extensions.Logging;

namespace WhiteTale.Server.Logging;

internal static partial class LoggerExtensions
{
	[LoggerMessage(LogEvent.None, LogLevel.Information, "{message}")]
	public static partial void AppLog(this ILogger logger, string message);
}
