namespace WhiteTale.Server.Common;

internal static partial class LoggerExtensions
{
	[LoggerMessage(LogLevel.Information, "{message}")]
	internal static partial void StartupMessage(this ILogger logger, String message);
}
