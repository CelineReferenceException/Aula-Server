namespace Aula.Server.Common.CommandLine;

internal sealed class CommandParameter
{
	internal const String Prefix = "-";

	internal required String Name { get; init; }

	internal required String Description { get; init; }

	internal Boolean IsRequired { get; init; } = true;

	internal Boolean RequiresArgument { get; init; } = true;

	internal Boolean CanOverflow { get; init; }
}
