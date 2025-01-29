namespace WhiteTale.Server.Domain.Bans;

internal sealed class Ban : DefaultDomainEntity
{
	internal UInt64 Id { get; private init; }

	internal BanType Type { get; private init; }

	internal UInt64? UserId { get; private init; }

	internal String? IpAddress { get; private init; }

	private Ban()
	{
	}

	internal static Ban Create(UInt64 id, BanType type, UInt64? userId = null, String? ipAddress = null)
	{
		var ban = new Ban
		{
			Id = id,
			Type = type,
			UserId = userId,
			IpAddress = ipAddress,
		};

		ban.AddEvent(new BanCreatedEvent(ban));
		return ban;
	}

	internal void Remove()
	{
		AddEvent(new BanRemovedEvent(this));
	}
}
