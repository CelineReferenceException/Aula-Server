using FluentValidation.Results;

namespace Aula.Server.Domain.Bans;

internal sealed class Ban : DefaultDomainEntity
{
	internal const Int32 ReasonMinimumLength = 1;
	internal const Int32 ReasonMaximumLength = 4096;

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

	internal static Result<Ban, ValidationFailure> Create(
		Snowflake id,
		BanType type,
		Snowflake? executorId = null,
		String? reason = null,
		Snowflake? targetId = null)
	{
		var ban = new Ban(id, type, executorId, reason, targetId, DateTime.UtcNow);
		ban.Events.Add(new BanCreatedEvent(ban));

		var validationResult = BanValidator.Instance.Validate(ban);
		return validationResult.IsValid
			? ban
			: new ResultErrorValues<ValidationFailure>(validationResult.Errors);
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
