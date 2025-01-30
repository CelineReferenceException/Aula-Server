using System.Collections.Concurrent;

namespace WhiteTale.Server.Common.CommandLine;

internal sealed class CommandLineService
{
	private readonly ConcurrentDictionary<String, Command> _commands = new();
	private readonly ILogger _logger;

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

			if (!segment.StartsWith(CommandParameter.Prefix)) // It is a subcommand
			{
				return await ProcessCommandAsync(input.Slice(segmentStart, input.Length - segmentStart), command.SubCommands,
					cancellationToken);
			}

			var parameterName = segment[CommandParameter.Prefix.Length..].ToString();
			if (!command.Parameters.TryGetValue(parameterName, out var parameter))
			{
				_logger.InvalidCommandParameter(parameterName);
				return false;
			}

			if (!parameter.HasInput)
			{
				arguments.Add(parameter.Name, String.Empty);
				continue;
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
				argumentLength = inputSegments.Current.End.Value - argumentStart;
			} while (parameter.CanOverflow &&
			         inputSegments.MoveNext());

			arguments.Add(parameter.Name, input.Slice(argumentStart, argumentLength).ToString());
		}

		await command.Callback(arguments, cancellationToken);
		return true;
	}
}
