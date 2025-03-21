namespace Aula.Server.Domain.Bans;

internal sealed class Ban : DefaultDomainEntity
{
	internal const Int32 ReasonMinimumLength = 1;
	internal const Int32 ReasonMaximumLength = 4096;

	internal Snowflake Id { get; }

	internal BanType Type { get; }

	internal Snowflake? ExecutorId { get; }

	internal String? Reason { get; }

	internal Snowflake? TargetId { get; }

	internal DateTime CreationDate { get; }

	internal Ban(
		Snowflake id,
		BanType type,
		Snowflake? executorId = null,
		String? reason = null,
		Snowflake? targetId = null)
	{
		if (!Enum.IsDefined(type))
		{
			throw new ArgumentOutOfRangeException(nameof(type));
		}

		if (type is BanType.Id &&
		    targetId is null)
		{
			throw new ArgumentException($"{nameof(targetId)} cannot be null when {nameof(type)} is {BanType.Id}.",
				nameof(targetId));
		}

		if (reason is not null)
		{
			switch (reason.Length)
			{
				case < ReasonMinimumLength:
					throw new ArgumentOutOfRangeException(nameof(reason),
						$"{nameof(reason)} length must be at least {ReasonMinimumLength}.");
				case > ReasonMaximumLength:
					throw new ArgumentOutOfRangeException(nameof(reason),
						$"{nameof(reason)} length must be at most ${ReasonMaximumLength}.");
				default: break;
			}
		}

		Id = id;
		Type = type;
		ExecutorId = executorId;
		Reason = reason;
		TargetId = targetId;
		CreationDate = DateTime.UtcNow;

		AddEvent(new BanCreatedEvent(this));
	}

	internal void Remove()
	{
		AddEvent(new BanRemovedEvent(this));
	}
}
