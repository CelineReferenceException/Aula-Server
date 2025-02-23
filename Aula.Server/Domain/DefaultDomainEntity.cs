namespace Aula.Server.Domain;

internal abstract class DefaultDomainEntity : IDomainEntity
{
	private readonly List<DomainEvent> _events = [];

	IReadOnlyList<DomainEvent> IDomainEntity.Events => _events;

	void IDomainEntity.ClearEvents()
	{
		_events.Clear();
	}

	private protected void AddEvent(DomainEvent domainEvent)
	{
		_events.Add(domainEvent);
	}
}
