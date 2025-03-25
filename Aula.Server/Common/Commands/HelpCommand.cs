using System.Text;

namespace Aula.Server.Common.Commands;

internal sealed partial class HelpCommand : Command
{
	private readonly CommandLine _commandLine;

	private readonly ILogger<HelpCommand> _logger;

	public HelpCommand(
		CommandLine commandLine,
		ILogger<HelpCommand> logger,
		IServiceProvider serviceProvider)
		: base(serviceProvider)
	{
		_commandLine = commandLine;
		_logger = logger;

		AddOptions(CommandOption);
	}

	internal override String Name => "help";

	internal override String Description => "Displays the list of available commands.";

	internal CommandOption CommandOption { get; } = new()
	{
		Name = "c",
		Description = "Show information about a specific command.",
		CanOverflow = true,
	};

	internal override ValueTask Callback(IReadOnlyDictionary<String, String> args, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		var commands = _commandLine.Commands
			.Select(static kvp => kvp.Value)
			.ToArray();

		if (!args.TryGetValue(CommandOption.Name, out var query) ||
		    String.IsNullOrWhiteSpace(query))
		{
			LogHelpMessage(_logger, CreateHelpMessage(commands));
			return ValueTask.CompletedTask;
		}

		var querySegments = query.Split(' ');
		var commandName = querySegments[0];
		if (!_commandLine.Commands.TryGetValue(commandName, out var command))
		{
			LogUnknownCommandMessage(_logger, commandName);
			return ValueTask.CompletedTask;
		}

		foreach (var subCommandName in querySegments.Skip(1))
		{
			if (!command.SubCommands.TryGetValue(subCommandName, out var subCommand))
			{
				LogUnknownSubCommandMessage(_logger, subCommandName);
				return ValueTask.CompletedTask;
			}

			command = subCommand;
		}

		LogHelpMessage(_logger, CreateHelpMessage(command));
		return ValueTask.CompletedTask;
	}

	internal ValueTask Callback(String commandName, CancellationToken cancellationToken = default)
	{
		return Callback(new Dictionary<String, String>
		{
			{ CommandOption.Name, commandName },
		}, cancellationToken);
	}

	private static String CreateHelpMessage(Command command)
	{
		var message = new StringBuilder();
		var alignment = 16;

		var parameters = new CommandParameters();

		foreach (var parameter in command.Options.Select(static kvp => kvp.Value))
		{
			var name = $"{CommandOption.Prefix}{parameter.Name}";
			parameters.Options.Add(new ParameterInfo(name, parameter.Description));

			if (name.Length > alignment)
			{
				alignment = name.Length;
			}
		}

		foreach (var subCommand in command.SubCommands.Select(kvp => kvp.Value))
		{
			var name = $"{subCommand.Name}";
			parameters.SubCommands.Add(new ParameterInfo($"{subCommand.Name}", subCommand.Description));

			if (subCommand.Name.Length > alignment)
			{
				alignment = name.Length;
			}
		}

		if (command.Name.Length > alignment)
		{
			alignment = command.Name.Length;
		}

		alignment++;

		const Int32 padding = 2;

		_ = message.AppendLine();
		_ = message.Append(command.Name);
		_ = message.AppendLine(command.Description.PadLeft(command.Description.Length + alignment - command.Name.Length));

		if (parameters.Options.Count > 0)
		{
			_ = message.AppendLine("OPTIONS: ");
		}

		foreach (var param in parameters.Options)
		{
			_ = message.Append(param.Name.PadLeft(param.Name.Length + padding));
			_ = message.AppendLine(param.Description.PadLeft(param.Description.Length + alignment - param.Name.Length - padding));
		}

		if (parameters.SubCommands.Count > 0)
		{
			_ = message.AppendLine();
			_ = message.AppendLine("SUB-COMMANDS: ");
		}

		foreach (var param in parameters.SubCommands)
		{
			_ = message.Append(param.Name.PadLeft(param.Name.Length + padding));
			_ = message.AppendLine(param.Description.PadLeft(param.Description.Length + alignment - param.Name.Length - padding));
		}

		return message.ToString();
	}

	private static String CreateHelpMessage(params ICollection<Command> commands)
	{
		var message = new StringBuilder();
		var alignment = commands
			.Select(command => command.Name.Length)
			.Prepend(16)
			.Max();

		alignment++;

		_ = message.AppendLine();
		foreach (var command in commands)
		{
			_ = message.Append($"{command.Name}");
			_ = message.AppendLine(command.Description.PadLeft(command.Description.Length + alignment - command.Name.Length));
		}

		return message.ToString();
	}

	[LoggerMessage(LogLevel.Information, Message = "Here's a list of all available commands: {message}")]
	private static partial void LogHelpMessage(ILogger logger, String message);

	[LoggerMessage(LogLevel.Error, Message = "Unknown command: '{commandName}'")]
	private static partial void LogUnknownCommandMessage(ILogger logger, String commandName);

	[LoggerMessage(LogLevel.Error, Message = "Unknown sub-command: '{commandName}'")]
	private static partial void LogUnknownSubCommandMessage(ILogger logger, String commandName);

	private readonly struct CommandParameters()
	{
		internal List<ParameterInfo> Options { get; } = [];

		internal List<ParameterInfo> SubCommands { get; } = [];
	}

	private readonly record struct ParameterInfo(String Name, String Description);
}
