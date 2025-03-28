﻿namespace Aula.Server.Core.Commands;

[CommandLineIgnore]
internal sealed class PermissionsSubCommand : Command
{
	public PermissionsSubCommand(IServiceProvider serviceProvider)
		: base(serviceProvider)
	{
		AddSubCommand<ListPermissionsSubCommand>();
		AddSubCommand<SetPermissionsSubCommand>();
	}

	internal override String Name => "permissions";

	internal override String Description => "Shows a list of user permissions related commands.";

	internal override async ValueTask Callback(IReadOnlyDictionary<String, String> args, CancellationToken ct)
	{
		var helpCommand = ServiceProvider.GetRequiredService<HelpCommand>();
		var userCommand = ServiceProvider.GetRequiredService<UserCommand>();
		await helpCommand.Callback($"{userCommand.Name} {Name}", ct);
	}
}
