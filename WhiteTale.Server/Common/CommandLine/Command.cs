namespace WhiteTale.Server.Common.CommandLine;

internal abstract class Command
{
	internal abstract String Name { get; }

	internal abstract String Description { get; }

	internal IReadOnlyDictionary<String, CommandParameter> Parameters { get; private set; } = new Dictionary<String, CommandParameter>();

	private protected void SetParameters(params IEnumerable<CommandParameter> parameters)
	{
		ArgumentNullException.ThrowIfNull(parameters, nameof(parameters));
		Parameters = ValidateCommandParameters(parameters).ToDictionary(p => p.Name);
	}

	internal abstract ValueTask Callback(IReadOnlyDictionary<String, String> args, CancellationToken cancellationToken);

	internal IReadOnlyDictionary<String, Command> SubCommands { get; private set; } = new Dictionary<String, Command>();

	private protected void SetSubCommands(params IEnumerable<Command> subCommands)
	{
		ArgumentNullException.ThrowIfNull(subCommands, nameof(subCommands));
		SubCommands = ValidateSubCommands(subCommands).ToDictionary(s => s.Name);
	}

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
