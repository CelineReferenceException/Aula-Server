namespace Aula.Server.Core.Features.Bans;

internal sealed record BanData
{
	public required BanType Type { get; init; }

	public Snowflake? ExecutorId { get; init; }

	public String? Reason { get; init; }

	public Snowflake? TargetId { get; init; }

	public required DateTime CreationDate { get; init; }
}
