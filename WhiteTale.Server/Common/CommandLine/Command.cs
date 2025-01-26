namespace WhiteTale.Server.Common.CommandLine;

internal abstract class Command : IDisposable
{
	private readonly Dictionary<String, Command> _subCommands = [];
	private readonly IServiceScope _serviceScope;

	private protected Command(IServiceProvider serviceProvider)
	{
		_serviceScope = serviceProvider.CreateScope();
	}

	internal abstract String Name { get; }

	internal abstract String Description { get; }

	internal IReadOnlyDictionary<String, CommandParameter> Parameters { get; private set; } = new Dictionary<String, CommandParameter>();

	internal IReadOnlyDictionary<String, Command> SubCommands => _subCommands;

	internal virtual ValueTask Callback(IReadOnlyDictionary<String, String> args, CancellationToken cancellationToken)
	{
		return ValueTask.CompletedTask;
	}

	private protected void SetParameters(params IEnumerable<CommandParameter> parameters)
	{
		ArgumentNullException.ThrowIfNull(parameters, nameof(parameters));
		Parameters = ValidateCommandParameters(parameters).ToDictionary(p => p.Name);
	}

	private protected void AddSubCommand(Type type)
	{
		var subCommand = (Command)_serviceScope.ServiceProvider.GetRequiredService(type);
		if (!_subCommands.TryAdd(subCommand.Name, subCommand))
		{
			throw new InvalidOperationException($"A subcommand with the name '{type.Name}' has already been registered.");
		}
	}

	private protected void AddSubCommand<TCommand>() where TCommand : Command
	{
		AddSubCommand(typeof(TCommand));
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

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	private protected virtual void Dispose(Boolean disposing)
	{
		if (disposing)
		{
			_serviceScope.Dispose();
		}
	}

	~Command()
	{
		Dispose(false);
	}
}
