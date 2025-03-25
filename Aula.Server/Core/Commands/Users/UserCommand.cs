namespace Aula.Server.Core.Commands.Users;

internal sealed class UserCommand : Command
{
	public UserCommand(IServiceProvider serviceProvider)
		: base(serviceProvider)
	{
	}

	internal override String Name => "user";

	internal override String Description => "User related commands.";

	internal override async ValueTask Callback(IReadOnlyDictionary<String, String> args, CancellationToken ct)
	{
		var help = ServiceProvider.GetRequiredService<HelpCommand>();
		await help.Callback(Name, ct);
	}
}
