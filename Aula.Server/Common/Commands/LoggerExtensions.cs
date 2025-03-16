namespace Aula.Server.Common.Commands;

internal static partial class LoggerExtensions
{
	[LoggerMessage(LogLevel.Error, Message = "Unknown command: '{name}'")]
	internal static partial void UnknownCommand(this ILogger logger, String name);

	[LoggerMessage(LogLevel.Error, Message = "Invalid command option name: '{name}'")]
	internal static partial void InvalidCommandOption(this ILogger logger, String name);

	[LoggerMessage(LogLevel.Error, Message = "Missing argument: '{optionName}'")]
	internal static partial void MissingArgument(this ILogger logger, String optionName);

	[LoggerMessage(LogLevel.Error, Message = "Command failed: {message}")]
	internal static partial void CommandFailed(this ILogger logger, String message);

	[LoggerMessage(LogLevel.Error, Message = "Command failed with an exception.")]
	internal static partial void CommandFailed(this ILogger logger, Exception exception);
}
