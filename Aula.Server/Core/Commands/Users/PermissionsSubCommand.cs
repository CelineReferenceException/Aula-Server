namespace Aula.Server.Core.Commands.Users;

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

	internal override String Description => "User permissions related commands.";
}
