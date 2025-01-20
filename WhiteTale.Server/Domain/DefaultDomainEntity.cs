namespace WhiteTale.Server.Domain;

internal abstract class DefaultDomainEntity : IDomainEntity
{
	private readonly List<DomainEvent> _events = [];

	public IReadOnlyList<DomainEvent> Events => _events;

	private protected void AddEvent(DomainEvent domainEvent)
	{
		_events.Add(domainEvent);
	}
}
