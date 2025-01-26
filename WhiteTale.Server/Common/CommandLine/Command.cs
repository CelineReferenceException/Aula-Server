using System.Collections.Immutable;

namespace WhiteTale.Server.Common.CommandLine;

internal sealed class Command
{
	public Command() : this([], [])
	{
	}

	public Command(IEnumerable<CommandParameter> parameters) : this(parameters, [])
	{
	}

	public Command(IEnumerable<Command> subCommands) : this([], subCommands)
	{
	}

	public Command(IEnumerable<CommandParameter> parameters, IEnumerable<Command> subCommands)
	{
		ArgumentNullException.ThrowIfNull(parameters, nameof(parameters));
		ArgumentNullException.ThrowIfNull(subCommands, nameof(subCommands));

		Parameters = ValidateCommandParameters(parameters).ToImmutableDictionary(p => p.Name);
		SubCommands = ValidateSubCommands(subCommands).ToImmutableDictionary(p => p.Name);
	}

	public required String Name { get; init; }

	public required String Description { get; init; }

	public ImmutableDictionary<String, CommandParameter> Parameters { get; }

	public required Func<IDictionary<String, String>, CancellationToken, ValueTask> Callback { get; init; }

	public ImmutableDictionary<String, Command> SubCommands { get; }

	private static IEnumerable<CommandParameter> ValidateCommandParameters(IEnumerable<CommandParameter> parameters)
	{
		var registeredNames = new HashSet<String>();

		CommandParameter? previousParam = null;
		foreach (var param in parameters)
		{
			if (previousParam is null)
			{
				previousParam = param;
				yield return param;
				continue;
			}

			if (!registeredNames.Add(param.Name))
			{
				throw new ArgumentException($"Duplicate command parameter name: '{param.Name}'.");
			}

			if (param.IsRequired &&
			    !previousParam.IsRequired)
			{
				throw new ArgumentException(
					$"An optional parameter cannot follow a required one. '{previousParam.Name}' is optional but '{param.Name}' is required.",
					nameof(parameters));
			}

			if (previousParam.CanOverflow)
			{
				throw new ArgumentException(
					$"The parameter '{previousParam.Name}' is marked for overflow, but is followed by another parameter.");
			}

			yield return param;
		}
	}

	private static IEnumerable<Command> ValidateSubCommands(IEnumerable<Command> subCommands)
	{
		var registeredNames = new HashSet<String>();

		foreach (var subCommand in subCommands)
		{
			if (!registeredNames.Add(subCommand.Name))
			{
				throw new InvalidOperationException($"Duplicate subcommand parameter name: '{subCommand.Name}'.");
			}

			yield return subCommand;
		}
	}
}
