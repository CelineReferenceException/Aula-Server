using Aula.Server.Domain;
using Aula.Server.Domain.Bans;

namespace Aula.Server.Core.Api.Bans;

internal sealed record BanData
{
	public required BanType Type { get; init; }

	public Snowflake? ExecutorId { get; init; }

	public String? Reason { get; init; }

	public Snowflake? TargetId { get; init; }

	public required DateTime CreationDate { get; init; }
}
