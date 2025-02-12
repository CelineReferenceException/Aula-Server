namespace Aula.Server.Common.CommandLine;

/// <summary>
///     A Command that is not automatically added to the <see cref="CommandLineService" /> by <see cref="DependencyInjection.MapCommands" />.
/// </summary>
internal abstract class SubCommand : Command
{
	private protected SubCommand(IServiceProvider serviceProvider) : base(serviceProvider)
	{
	}
}
