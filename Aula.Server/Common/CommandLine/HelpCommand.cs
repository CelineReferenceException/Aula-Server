using System.Text;

namespace Aula.Server.Common.CommandLine;

internal sealed partial class HelpCommand : Command
{
	private readonly CommandLineService _commandLineService;

	private readonly CommandParameter _commandOption = new()
	{
		Name = "c",
		Description = "Show information about a specific command.",
		CanOverflow = true,
	};

	private readonly ILogger<HelpCommand> _logger;


	public HelpCommand(
		CommandLineService commandLineService,
		ILogger<HelpCommand> logger,
		IServiceProvider serviceProvider) : base(serviceProvider)
	{
		_commandLineService = commandLineService;
		_logger = logger;

		AddOptions(_commandOption);
	}

	internal override String Name => "help";

	internal override String Description => "Displays the list of available commands.";

	internal override ValueTask Callback(IReadOnlyDictionary<String, String> args, CancellationToken ct)
	{
		ct.ThrowIfCancellationRequested();

		var commands = _commandLineService.Commands
			.Select(kvp => kvp.Value)
			.ToList();

		if (!args.TryGetValue(_commandOption.Name, out var query) ||
		    String.IsNullOrWhiteSpace(query))
		{
			ShowHelp(_logger, FormatCommands(commands));
			return ValueTask.CompletedTask;
		}

		var querySegments = query.Split(' ');
		var command = _commandLineService.Commands[querySegments[0]];

		foreach (var subCommandName in querySegments.Skip(1))
		{
			if (!command.SubCommands.TryGetValue(subCommandName, out var subCommand))
			{
				ShowHelp(_logger, $"Unknown subcommand '{subCommandName}'.");
				return ValueTask.CompletedTask;
			}

			command = subCommand;
		}

		ShowHelp(_logger, FormatCommands(command));
		return ValueTask.CompletedTask;
	}

	private static String FormatCommands(Command command)
	{
		var message = new StringBuilder();
		var alignment = 16;

		var parameters = new CommandParameters();

		foreach (var parameter in command.Options.Select(kvp => kvp.Value))
		{
			var name = $"{CommandParameter.Prefix}{parameter.Name}";
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
			_ = message.AppendLine("Options: ");
		}

		foreach (var param in parameters.Options)
		{
			_ = message.Append(param.Name.PadLeft(param.Name.Length + padding));
			_ = message.AppendLine(param.Description.PadLeft(param.Description.Length + alignment - param.Name.Length - padding));
		}

		if (parameters.SubCommands.Count > 0)
		{
			_ = message.AppendLine();
			_ = message.AppendLine("Sub-commands: ");
		}

		foreach (var param in parameters.SubCommands)
		{
			_ = message.Append(param.Name.PadLeft(param.Name.Length + padding));
			_ = message.AppendLine(param.Description.PadLeft(param.Description.Length + alignment - param.Name.Length - padding));
		}

		return message.ToString();
	}

	private static String FormatCommands(params ICollection<Command> commands)
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
	private static partial void ShowHelp(ILogger logger, String message);

	private sealed record CommandParameters
	{
		internal List<ParameterInfo> Options { get; } = [];
		internal List<ParameterInfo> SubCommands { get; } = [];
	}

	private readonly record struct ParameterInfo(String Name, String Description);
}
