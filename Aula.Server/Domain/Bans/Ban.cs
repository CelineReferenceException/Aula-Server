namespace Aula.Server.Domain.Bans;

internal sealed class Ban : DefaultDomainEntity
{
	internal const Int32 ReasonMinimumLength = 1;
	internal const Int32 ReasonMaximumLength = 4096;

	private static readonly ResultProblem s_unknownBanType =
		new("Unknown ban type", "An unknown ban type was provided.");

	private static readonly ResultProblem s_idBanWithNullId =
		new("Invalid id", "Id bans are required to define a user id.");

	private static readonly ResultProblem s_reasonTooShort =
		new("Reason is too short", $"Reason length must be at least {ReasonMinimumLength}.");

	private static readonly ResultProblem s_reasonTooLong =
		new("Reason is too long", $"Reason length must be at most {ReasonMaximumLength}.");

	private Ban(Snowflake id, BanType type, Snowflake? executorId, String? reason, Snowflake? targetId, DateTime creationDate)
	{
		Id = id;
		Type = type;
		ExecutorId = executorId;
		Reason = reason;
		TargetId = targetId;
		CreationDate = creationDate;
	}

	internal Snowflake Id { get; }

	internal BanType Type { get; }

	internal Snowflake? ExecutorId { get; }

	internal String? Reason { get; }

	internal Snowflake? TargetId { get; }

	internal DateTime CreationDate { get; }

	internal static Result<Ban> Create(
		Snowflake id,
		BanType type,
		Snowflake? executorId = null,
		String? reason = null,
		Snowflake? targetId = null)
	{
		var problems = new Items<ResultProblem>();

		if (!Enum.IsDefined(type))
		{
			problems.Add(s_unknownBanType);
		}

		if (type is BanType.Id &&
		    targetId is null)
		{
			problems.Add(s_idBanWithNullId);
		}

		if (reason is not null)
		{
			switch (reason.Length)
			{
				case < ReasonMinimumLength: problems.Add(s_reasonTooShort); break;
				case > ReasonMaximumLength: problems.Add(s_reasonTooLong); break;
				default: break;
			}
		}

		if (problems.Count > 0)
		{
			return new ResultProblemValues(problems);
		}

		var ban = new Ban(id, type, executorId, reason, targetId, DateTime.UtcNow);
		ban.Events.Add(new BanCreatedEvent(ban));
		return ban;
	}

	internal void Remove()
	{
		if (Events.Any(e => e is BanRemovedEvent))
		{
			return;
		}

		Events.Add(new BanRemovedEvent(this));
	}
}
