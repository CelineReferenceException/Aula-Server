namespace Aula.Server.Common.CommandLine;

/// <summary>
///     Provides a structure for command execution.
/// </summary>
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

	/// <summary>
	///     The name of the command.
	/// </summary>
	internal abstract String Name { get; }

	/// <summary>
	///     The description of the command.
	/// </summary>
	internal abstract String Description { get; }

	/// <summary>
	///     A dictionary with the registered options, where the <see cref="CommandOption.Name" /> is the key.
	/// </summary>
	internal IReadOnlyDictionary<String, CommandOption> Options => _options;

	/// <summary>
	///     A dictionary with the registered the sub-commands, where the <see cref="SubCommand.Name" /> is the key.
	/// </summary>
	internal IReadOnlyDictionary<String, Command> SubCommands => _subCommands;

	/// <summary>
	///     The callback function called each time the command is executed.
	/// </summary>
	/// <param name="args">A dictionary where option names are the key, and their corresponding user input are the value.</param>
	/// <param name="cancellationToken">A cancellation token to observe while executing the command</param>
	/// <returns>A task that resolves when the callback completes.</returns>
	internal virtual ValueTask Callback(IReadOnlyDictionary<String, String> args, CancellationToken cancellationToken)
	{
		return ValueTask.CompletedTask;
	}

	/// <summary>
	///     Adds a <see cref="CommandOption" /> to this <see cref="Command" /> instance.
	/// </summary>
	/// <param name="option">The option to register.</param>
	/// <exception cref="InvalidOperationException">Thrown if an option with the same name is already registered.</exception>
	/// <exception cref="InvalidOperationException">Thrown if the last option registered was marked as optional and this is marked as required.</exception>
	/// <exception cref="InvalidOperationException">Thrown if the last option registered was marked for overflow.</exception>
	private protected void AddOptions(CommandOption option)
	{
		ArgumentNullException.ThrowIfNull(option, nameof(option));

		if (!_options.TryAdd(option.Name, option))
		{
			throw new InvalidOperationException($"Duplicate command option name: '{option.Name}'.");
		}

		if (_previousDefinedOption is null)
		{
			return;
		}

		if (option.IsRequired &&
		    !_previousDefinedOption.IsRequired)
		{
			throw new InvalidOperationException(
				$"An optional option parameter cannot follow a required one. '{_previousDefinedOption.Name}' is optional but '{option.Name}' is required.");
		}

		if (_previousDefinedOption.CanOverflow)
		{
			throw new InvalidOperationException(
				$"The option '{_previousDefinedOption.Name}' is marked for overflow, but is followed by another option.");
		}

		_previousDefinedOption = option;
	}

	/// <summary>
	///     Adds an enumeration of <see cref="CommandOption" /> to this <see cref="Command" /> instance.
	/// </summary>
	/// <param name="parameters">The options to register.</param>
	private protected void AddOptions(params IEnumerable<CommandOption> parameters)
	{
		foreach (var parameter in parameters)
		{
			AddOptions(parameter);
		}
	}

	/// <summary>
	///     Adds a <see cref="SubCommand" /> to this <see cref="Command" /> instance.
	/// </summary>
	/// <param name="type">The <see cref="Type" /> of the subcommand class.</param>
	/// <exception cref="InvalidOperationException">A subcommand with the same name is already registered.</exception>
	private protected void AddSubCommand(Type type)
	{
		var subCommand = (Command)_serviceProvider.GetRequiredService(type);
		if (!_subCommands.TryAdd(subCommand.Name, subCommand))
		{
			throw new InvalidOperationException($"A subcommand with the name '{type.Name}' has already been registered.");
		}
	}

	/// <summary>
	///     Registers a <see cref="SubCommand" /> for this <see cref="Command" /> instance.
	/// </summary>
	/// <typeparam name="TCommand">The subcommand class.</typeparam>
	/// <exception cref="InvalidOperationException">A subcommand with the same name is already registered.</exception>
	private protected void AddSubCommand<TCommand>() where TCommand : Command
	{
		AddSubCommand(typeof(TCommand));
	}
}
