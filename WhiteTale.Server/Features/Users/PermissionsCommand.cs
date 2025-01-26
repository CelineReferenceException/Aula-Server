namespace WhiteTale.Server.Features.Users;

internal sealed class PermissionsCommand : SubCommand
{
	public PermissionsCommand(IServiceProvider serviceProvider) : base(serviceProvider)
	{
		AddSubCommand<SetPermissionsSubCommand>();
	}

	internal override String Name => "permissions";

	internal override String Description => "User permissions related commands.";
}
