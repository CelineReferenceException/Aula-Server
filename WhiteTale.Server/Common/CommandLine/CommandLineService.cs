using System.Collections.Concurrent;
using System.Text;

namespace WhiteTale.Server.Common.CommandLine;

internal sealed class CommandLineService
{
	private const String ParameterPrefix = "-";
	private readonly ConcurrentDictionary<String, Command> _commands = new();
	private readonly ILogger _logger;

	public CommandLineService(ILogger<CommandLineService> logger)
	{
		_logger = logger;
		AddCommand(new Command([
			new CommandParameter
			{
				Name = "c",
				Description = "Show information about a specific command.",
				CanOverflow = true,
			},
		])
		{
			Name = "help",
			Description = "Displays the list of available commands.",
			Callback = HelpCommandCallback,
		});
	}

	internal IReadOnlyDictionary<String, Command> Commands => _commands;

	internal void AddCommand(Command command)
	{
		if (!_commands.TryAdd(command.Name, command))
		{
			throw new InvalidOperationException($"Command name already registered: '{command.Name}'.");
		}
	}

	internal async ValueTask<Boolean> ProcessCommandAsync(ReadOnlyMemory<Char> input, CancellationToken ct = default)
	{
		return await ProcessCommandAsync(input, _commands, ct);
	}

	private async ValueTask<Boolean> ProcessCommandAsync(
		ReadOnlyMemory<Char> input,
		IDictionary<String, Command> commands,
		CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		var inputSpan = input.Span;
		if (inputSpan.IsWhiteSpace())
		{
			return false;
		}

		var inputSegments = inputSpan.Split(' ');
		if (!inputSegments.MoveNext())
		{
			return false;
		}

		var commandName = inputSpan.Slice(inputSegments.Current.Start.Value, inputSegments.Current.End.Value).ToString();
		if (!commands.TryGetValue(commandName, out var command))
		{
			_logger.UnknownCommand(commandName);
			return false;
		}

		var arguments = new Dictionary<String, String>();
		while (inputSegments.MoveNext())
		{
			cancellationToken.ThrowIfCancellationRequested();
			var segmentStart = inputSegments.Current.Start.Value;
			var segmentLength = inputSegments.Current.End.Value - segmentStart;

			if (segmentLength == 0)
			{
				continue;
			}

			var segment = input.Span.Slice(segmentStart, segmentLength);

			if (segmentStart == 0)
			{
				commandName = segment.ToString();
				continue;
			}

			if (!segment.StartsWith(ParameterPrefix))
			{
				return await ProcessCommandAsync(input.Slice(segmentStart, input.Length - segmentStart), command.SubCommands,
					cancellationToken);
			}

			var parameterName = segment[ParameterPrefix.Length..].ToString();
			CommandParameter? parameter = null;
			foreach (var param in commands[commandName].Parameters.Select(kvp => kvp.Value))
			{
				if (param.Name == parameterName)
				{
					parameter = param;
				}
			}

			if (parameter is null)
			{
				_logger.InvalidCommandParameter(parameterName);
				return false;
			}

			if (!parameter.RequiresInput)
			{
				arguments.Add(parameter.Name, String.Empty);
			}

			var hasArguments = inputSegments.MoveNext();
			if (!hasArguments)
			{
				_logger.MissingArgument(parameterName);
				return false;
			}

			var argumentStart = inputSegments.Current.Start.Value;
			var argumentLength = 0;
			do
			{
				argumentLength = argumentLength + inputSegments.Current.End.Value - argumentStart;
			} while (inputSegments.MoveNext() &&
			         parameter.CanOverflow);

			arguments.Add(parameter.Name, input.Slice(argumentStart, argumentLength).ToString());
		}

		await command.Callback(arguments, cancellationToken);
		return true;
	}

	private ValueTask HelpCommandCallback(IDictionary<String, String> args, CancellationToken ct)
	{
		ct.ThrowIfCancellationRequested();

		var commands = _commands.Select(kvp => kvp.Value);

		if (!args.TryGetValue("command", out var query) ||
		    String.IsNullOrWhiteSpace(query))
		{
			_logger.ShowHelp(FormatCommands(commands));
			return ValueTask.CompletedTask;
		}

		var querySegments = query.Split(' ');
		var command = _commands[querySegments[0]];

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
				var name = $"{ParameterPrefix}{parameter.Name}";
				paramsInfo.Add(new ParameterInfo(name, parameter.Description));

				if (name.Length > alignment)
				{
					alignment = name.Length;
				}
			}

			foreach (var subCommand in command.SubCommands.Select(kvp => kvp.Value))
			{
				var name = $"{subCommand.Name}";
				paramsInfo.Add(new ParameterInfo($"  {subCommand.Name}", subCommand.Description));

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
