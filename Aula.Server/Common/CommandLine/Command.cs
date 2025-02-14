namespace Aula.Server.Common.CommandLine;

internal abstract class Command
{
	private readonly IServiceProvider _serviceProvider;
	private readonly Dictionary<String, CommandOption> _options = [];
	private readonly Dictionary<String, Command> _subCommands = [];
	private CommandOption? _previousDefinedOption;

	private protected Command(IServiceProvider serviceProvider)
	{
		_serviceProvider = serviceProvider;
	}

	internal abstract String Name { get; }

	internal abstract String Description { get; }

	internal IReadOnlyDictionary<String, CommandOption> Options => _options;

	internal IReadOnlyDictionary<String, Command> SubCommands => _subCommands;

	internal virtual ValueTask Callback(IReadOnlyDictionary<String, String> args, CancellationToken cancellationToken)
	{
		return ValueTask.CompletedTask;
	}

	private protected void AddOptions(CommandOption option)
	{
		ArgumentNullException.ThrowIfNull(option, nameof(option));

		if (!_options.TryAdd(option.Name, option))
		{
			throw new ArgumentException($"Duplicate command option name: '{option.Name}'.", nameof(option));
		}

		if (_previousDefinedOption is null)
		{
			return;
		}

		if (option.IsRequired &&
		    !_previousDefinedOption.IsRequired)
		{
			throw new ArgumentException(
				$"An optional option parameter cannot follow a required one. '{_previousDefinedOption.Name}' is optional but '{option.Name}' is required.",
				nameof(option));
		}

		if (_previousDefinedOption.CanOverflow)
		{
			throw new ArgumentException(
				$"The option '{_previousDefinedOption.Name}' is marked for overflow, but is followed by another option.",
				nameof(option));
		}

		_previousDefinedOption = option;
	}

	private protected void AddOptions(params IEnumerable<CommandOption> parameters)
	{
		foreach (var parameter in parameters)
		{
			AddOptions(parameter);
		}
	}

	private protected void AddSubCommand(Type type)
	{
		var subCommand = (Command)_serviceProvider.GetRequiredService(type);
		if (!_subCommands.TryAdd(subCommand.Name, subCommand))
		{
			throw new InvalidOperationException($"A subcommand with the name '{type.Name}' has already been registered.");
		}
	}

	private protected void AddSubCommand<TCommand>() where TCommand : Command
	{
		AddSubCommand(typeof(TCommand));
	}
}
