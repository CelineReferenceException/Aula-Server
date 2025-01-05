using System.ComponentModel.DataAnnotations;

namespace WhiteTale.Server.Common.Persistence;

internal sealed class PersistenceOptions
{
	internal const String SectionName = "Persistence";

	[Required]
	public required Boolean UseInMemoryDatabase { get; set; }
}
