namespace WhiteTale.Server.Common.CommandLine;

internal abstract class Command : IDisposable
{
	private readonly Dictionary<String, CommandParameter> _options = [];
	private readonly IServiceScope _serviceScope;
	private readonly Dictionary<String, Command> _subCommands = [];
	private CommandParameter? _previousDefinedParameter;

	private protected Command(IServiceProvider serviceProvider)
	{
		_serviceScope = serviceProvider.CreateScope();
	}

	internal abstract String Name { get; }

	internal abstract String Description { get; }

	internal IReadOnlyDictionary<String, CommandParameter> Options => _options;

	internal IReadOnlyDictionary<String, Command> SubCommands => _subCommands;

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	internal virtual ValueTask Callback(IReadOnlyDictionary<String, String> args, CancellationToken cancellationToken)
	{
		return ValueTask.CompletedTask;
	}

	private protected void AddOptions(CommandParameter parameter)
	{
		ArgumentNullException.ThrowIfNull(parameter, nameof(parameter));

		if (!_options.TryAdd(parameter.Name, parameter))
		{
			throw new ArgumentException($"Duplicate command option name: '{parameter.Name}'.", nameof(parameter));
		}

		if (_previousDefinedParameter is null)
		{
			return;
		}

		if (parameter.IsRequired &&
		    !_previousDefinedParameter.IsRequired)
		{
			throw new ArgumentException(
				$"An optional option parameter cannot follow a required one. '{_previousDefinedParameter.Name}' is optional but '{parameter.Name}' is required.",
				nameof(parameter));
		}

		if (_previousDefinedParameter.CanOverflow)
		{
			throw new ArgumentException(
				$"The option '{_previousDefinedParameter.Name}' is marked for overflow, but is followed by another option.",
				nameof(parameter));
		}

		_previousDefinedParameter = parameter;
	}

	private protected void AddOptions(params IEnumerable<CommandParameter> parameters)
	{
		foreach (var parameter in parameters)
		{
			AddOptions(parameter);
		}
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
