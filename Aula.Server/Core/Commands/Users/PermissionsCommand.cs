namespace Aula.Server.Core.Commands.Users;

[CommandLineIgnore]
internal sealed class PermissionsCommand : Command
{
	public PermissionsCommand(IServiceProvider serviceProvider)
		: base(serviceProvider)
	{
		AddSubCommand<ListPermissionsCommand>();
		AddSubCommand<SetPermissionsSubCommand>();
	}

	internal override String Name => "permissions";

	internal override String Description => "User permissions related commands.";
}
