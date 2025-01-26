namespace WhiteTale.Server.Common.CommandLine;

internal sealed class CommandParameter
{
	internal const String Prefix = "-";

	public required String Name { get; init; }

	public required String Description { get; init; }

	public Boolean IsRequired { get; init; } = true;

	public Boolean RequiresInput { get; init; } = true;

	public Boolean CanOverflow { get; init; }
}
