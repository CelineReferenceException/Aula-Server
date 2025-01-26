namespace WhiteTale.Server.Common.CommandLine;

internal static partial class LoggerExtensions
{
	[LoggerMessage(LogLevel.Error, Message = "Unknown command: '{name}'")]
	internal static partial void UnknownCommand(this ILogger logger, String name);

	[LoggerMessage(LogLevel.Error, Message = "Invalid command parameter name: '{name}'")]
	internal static partial void InvalidCommandParameter(this ILogger logger, String name);

	[LoggerMessage(LogLevel.Error, Message = "Missing argument: '{parameterName}'")]
	internal static partial void MissingArgument(this ILogger logger, String parameterName);

	[LoggerMessage(LogLevel.Information, Message = "{message}")]
	internal static partial void ShowHelp(this ILogger logger, String message);

	[LoggerMessage(LogLevel.Error, Message = "Command failed during execution: {message}")]
	internal static partial void CommandFailed(this ILogger logger, String message);
}
