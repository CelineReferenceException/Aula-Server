namespace Aula.Server.Features.Bans;

internal sealed record BanData
{
	public required BanType Type { get; init; }

	public UInt64? ExecutorId { get; init; }

	public String? Reason { get; init; }

	public UInt64? TargetId { get; init; }

	public required DateTime CreationTime { get; init; }
}
