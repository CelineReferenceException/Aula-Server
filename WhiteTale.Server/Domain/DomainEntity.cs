namespace WhiteTale.Server.Domain;

internal abstract class DomainEntity
{
	private readonly List<DomainEvent> _domainEvents = [];

	internal IReadOnlyList<DomainEvent> DomainEvents => _domainEvents;

	private protected void AddEvent(DomainEvent domainEvent)
	{
		_domainEvents.Add(domainEvent);
	}
}
