namespace Aula.Server.Core;

internal static partial class LoggerExtensions
{
	[LoggerMessage(LogLevel.Information, "{message}")]
	internal static partial void LogStartupMessage(this ILogger logger, String message);
}
