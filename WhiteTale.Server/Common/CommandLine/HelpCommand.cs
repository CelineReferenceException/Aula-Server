using System.Text;

namespace WhiteTale.Server.Common.CommandLine;

internal sealed class HelpCommand : Command
{
	private readonly CommandLineService _commandLineService;
	private readonly ILogger<HelpCommand> _logger;

	private readonly CommandParameter _commandParameter = new()
	{
		Name = "c",
		Description = "Show information about a specific command.",
		CanOverflow = true,
	};

	internal override String Name => "help";

	internal override String Description => "Displays the list of available commands.";


	public HelpCommand(
		CommandLineService commandLineService,
		ILogger<HelpCommand> logger,
		IServiceProvider serviceProvider) : base(serviceProvider)
	{
		_commandLineService = commandLineService;
		_logger = logger;

		SetParameters(_commandParameter);
	}

	internal override ValueTask Callback(IReadOnlyDictionary<String, String> args, CancellationToken ct)
	{
		ct.ThrowIfCancellationRequested();

		var commands = _commandLineService.Commands.Select(kvp => kvp.Value);

		if (!args.TryGetValue(_commandParameter.Name, out var query) ||
		    String.IsNullOrWhiteSpace(query))
		{
			_logger.ShowHelp(FormatCommands(commands));
			return ValueTask.CompletedTask;
		}

		var querySegments = query.Split(' ');
		var command = _commandLineService.Commands[querySegments[0]];

		foreach (var subCommandName in querySegments.Skip(1))
		{
			if (!command.SubCommands.TryGetValue(subCommandName, out var subCommand))
			{
				_logger.ShowHelp($"Unknown subcommand '{subCommandName}'.");
				return ValueTask.CompletedTask;
			}

			command = subCommand;
		}

		_logger.ShowHelp(FormatCommands(command));
		return ValueTask.CompletedTask;
	}

	private static String FormatCommands(params IEnumerable<Command> commands)
	{
		var message = new StringBuilder();
		var info = new Dictionary<Command, List<ParameterInfo>>();
		var alignment = 16;

		foreach (var command in commands)
		{
			var paramsInfo = new List<ParameterInfo>();

			foreach (var parameter in command.Parameters.Select(kvp => kvp.Value))
			{
				var name = $"{CommandParameter.Prefix}{parameter.Name}";
				paramsInfo.Add(new ParameterInfo(name, parameter.Description));

				if (name.Length > alignment)
				{
					alignment = name.Length;
				}
			}

			foreach (var subCommand in command.SubCommands.Select(kvp => kvp.Value))
			{
				var name = $"{subCommand.Name}";
				paramsInfo.Add(new ParameterInfo($"{subCommand.Name}", subCommand.Description));

				if (subCommand.Name.Length > alignment)
				{
					alignment = name.Length;
				}
			}

			if (command.Name.Length > alignment)
			{
				alignment = command.Name.Length;
			}

			info.Add(command, paramsInfo);
		}

		alignment++;

		_ = message.AppendLine();
		foreach (var (command, parameters) in info)
		{
			_ = message.Append(command.Name);
			_ = message.AppendLine(command.Description.PadLeft(command.Description.Length + alignment - command.Name.Length));

			foreach (var param in parameters)
			{
				const Int32 padding = 2;
				_ = message.Append(param.Name.PadLeft(param.Name.Length + padding));
				_ = message.AppendLine(param.Description.PadLeft(param.Description.Length + alignment - param.Name.Length - padding));
			}
		}

		return message.ToString();
	}

	private readonly record struct ParameterInfo(String Name, String Description);
}
