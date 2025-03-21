namespace Aula.Server.Core.Features.Users.Commands;

internal sealed class PermissionsCommand : SubCommand
{
	public PermissionsCommand(IServiceProvider serviceProvider) : base(serviceProvider)
	{
		AddSubCommand<ListPermissionsCommand>();
		AddSubCommand<SetPermissionsSubCommand>();
	}

	internal override String Name => "permissions";

	internal override String Description => "User permissions related commands.";
}
