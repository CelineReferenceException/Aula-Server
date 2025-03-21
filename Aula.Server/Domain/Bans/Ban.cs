namespace Aula.Server.Domain.Bans;

internal sealed class Ban : DefaultDomainEntity
{
	internal const Int32 ReasonMinimumLength = 1;
	internal const Int32 ReasonMaximumLength = 4096;

	internal UInt64 Id { get; }

	internal BanType Type { get; }

	internal UInt64? ExecutorId { get; }

	internal String? Reason { get; }

	internal UInt64? TargetId { get; }

	internal DateTime CreationDate { get; }

	internal Ban(
		UInt64 id,
		BanType type,
		UInt64? executorId = null,
		String? reason = null,
		UInt64? targetId = null)
	{
		if (id is 0)
		{
			throw new ArgumentException($"{nameof(id)} cannot be 0.", nameof(id));
		}

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

		if (executorId is 0)
		{
			throw new ArgumentException($"{nameof(executorId)} cannot be 0.", nameof(executorId));
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
