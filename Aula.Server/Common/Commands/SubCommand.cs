namespace Aula.Server.Common.Commands;

/// <summary>
///     A Command that is not automatically added to the <see cref="CommandLine" /> by <see cref="DependencyInjection.MapCommands" />.
/// </summary>
internal abstract class SubCommand : Command
{
	private protected SubCommand(IServiceProvider serviceProvider) : base(serviceProvider)
	{
	}
}
