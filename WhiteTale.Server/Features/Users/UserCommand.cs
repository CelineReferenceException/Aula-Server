namespace WhiteTale.Server.Features.Users;

internal sealed class UserCommand : Command
{
	public UserCommand(IServiceProvider serviceProvider) : base(serviceProvider)
	{
		AddSubCommand<PermissionsCommand>();
	}

	internal override String Name => "user";

	internal override String Description => "User related commands.";
}
