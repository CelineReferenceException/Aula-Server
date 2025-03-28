using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Aula.Server.Common.Cache;

/// <summary>
///     Cache related configurations.
/// </summary>
internal sealed class CacheOptions
{
	internal const String SectionName = "Cache";

	/// <summary>
	///     The connection string used to connect to a Redis instance.
	/// </summary>
	[Required]
	[NotNull]
	public String? ConnectionString { get; set; }

	/**
	 * The Redis instance name.
	 * Allows partitioning a single backend cache for use with multiple apps/services.
	 * If set, the cache keys are prefixed with this value.
	 */
	public String? InstanceName { get; set; }
}
