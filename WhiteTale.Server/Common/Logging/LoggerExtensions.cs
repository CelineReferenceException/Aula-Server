using Microsoft.Extensions.Logging;

namespace WhiteTale.Server.Common.Logging;

internal static partial class LoggerExtensions
{
	[LoggerMessage(LogLevel.Information, "{message}")]
	public static partial void LogMessage(this ILogger logger, string message);
}
