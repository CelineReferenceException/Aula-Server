namespace WhiteTale.Server.Domain.Bans;

internal sealed class Ban : DefaultDomainEntity
{
	internal const Int32 ReasonMinimumLength = 1;
	internal const Int32 ReasonMaximumLength = 512;

	internal UInt64 Id { get; private init; }

	internal BanType Type { get; private init; }

	internal UInt64? ExecutorId { get; private init; }

	internal String? Reason { get; private init; }

	internal UInt64? TargetId { get; private init; }

	internal String? IpAddress { get; private init; }

	internal DateTime CreationTime { get; private init; }

	private Ban()
	{
	}

	internal static Ban Create(
		UInt64 id,
		BanType type,
		UInt64? executorId = null,
		String? reason = null,
		UInt64? targetId = null,
		String? ipAddress = null)
	{
		var ban = new Ban
		{
			Id = id,
			Type = type,
			ExecutorId = executorId,
			Reason = reason,
			TargetId = targetId,
			IpAddress = ipAddress,
			CreationTime = DateTime.Now,
		};

		ban.AddEvent(new BanCreatedEvent(ban));
		return ban;
	}

	internal void Remove()
	{
		AddEvent(new BanRemovedEvent(this));
	}
}
