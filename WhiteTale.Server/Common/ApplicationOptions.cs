using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace WhiteTale.Server.Common;

/// <summary>
///     General configurations for the application.
/// </summary>
internal sealed class ApplicationOptions
{
	internal const String SectionName = "Application";

	/// <summary>
	///     The publicly displayed name.
	/// </summary>
	[Required]
	public required String Name { get; set; }

	/// <summary>
	///     The identifier for this worker, must be unique.
	/// </summary>
	[Required]
	[NotNull]
	[Range(0, 31)]
	public required UInt32? WorkerId { get; set; }
}
