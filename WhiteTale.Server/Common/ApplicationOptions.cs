using System.ComponentModel.DataAnnotations;

namespace WhiteTale.Server.Common;

internal sealed class ApplicationOptions
{
	internal const String SectionName = "Application";

	[Required]
	public required String Name { get; set; }

	[Required]
	[Range(0, 31)]
	public required UInt32 WorkerId { get; set; }
}
