using System.Collections.Concurrent;

namespace Aula.Server.Common.CommandLine;

internal sealed class CommandLineService
{
	private readonly ConcurrentDictionary<String, Command> _commands = new();
	private readonly ILogger<CommandLineService> _logger;

	public CommandLineService(ILogger<CommandLineService> logger)
	{
		_logger = logger;
	}

	internal IReadOnlyDictionary<String, Command> Commands => _commands;

	internal void AddCommand(Command command)
	{
		if (!_commands.TryAdd(command.Name, command))
		{
			throw new InvalidOperationException($"Command name already registered: '{command.Name}'.");
		}
	}

	internal async ValueTask<Boolean> ProcessCommandAsync(ReadOnlyMemory<Char> input, CancellationToken cancellationToken = default)
	{
		return await ProcessCommandAsync(input, _commands, cancellationToken);
	}

	private async ValueTask<Boolean> ProcessCommandAsync(
		ReadOnlyMemory<Char> input,
		IReadOnlyDictionary<String, Command> commands,
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

			var startsWithParameterPrefix = segment.StartsWith(CommandParameter.Prefix);
			if (!startsWithParameterPrefix &&
			    command.SubCommands.Count is not 0)
			{
				// Should be a subcommand
				return await ProcessCommandAsync(input.Slice(segmentStart, input.Length - segmentStart), command.SubCommands,
					cancellationToken);
			}

			CommandParameter? option;
			if (startsWithParameterPrefix)
			{
				var optionName = segment[CommandParameter.Prefix.Length..].ToString();
				if (!command.Options.TryGetValue(optionName, out option))
				{
					_logger.InvalidCommandParameter(optionName);
					return false;
				}

				if (option.RequiresArgument &&
				    !inputSegments.MoveNext())
				{
					_logger.MissingArgument(option.Name);
					return false;
				}

				arguments.Add(option.Name, String.Empty);
				continue;
			}

			if (command.Options.Count is 1)
			{
				// If a command has no subcommands, only one parameter, and no parameter name is provided,
				// then that parameter is automatically selected.
				option = command.Options
					.Select(kvp => kvp.Value)
					.First();
			}
			else
			{
				// The command multiple parameters, and we cannot guess which select.
				// returns the same response for unrecognized subcommands.
				_logger.UnknownCommand(commandName);
				return false;
			}

			var argumentStart = inputSegments.Current.Start.Value;
			var argumentLength = inputSegments.Current.End.Value - argumentStart;
			if (option.CanOverflow)
			{
				while (inputSegments.MoveNext())
				{
					argumentLength = inputSegments.Current.End.Value - argumentStart;
				}
			}

			arguments.Add(option.Name, input.Slice(argumentStart, argumentLength).ToString());
		}

		await command.Callback(arguments, cancellationToken);
		return true;
	}
}
