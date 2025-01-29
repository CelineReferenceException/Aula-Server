namespace WhiteTale.Server.Common;

internal static partial class LoggerExtensions
{
	[LoggerMessage(LogLevel.Information, "{message}")]
	internal static partial void Inform(this ILogger logger, String message);

	[LoggerMessage(LogLevel.Trace, "{message}")]
	internal static partial void Trace(this ILogger logger, String message);
}
