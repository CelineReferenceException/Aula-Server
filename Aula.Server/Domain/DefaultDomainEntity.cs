namespace Aula.Server.Domain;

internal abstract class DefaultDomainEntity : IDomainEntity
{
	private protected List<DomainEvent> Events { get; } = [];
	IReadOnlyList<DomainEvent> IDomainEntity.Events => Events;

	void IDomainEntity.ClearEvents()
	{
		Events.Clear();
	}
}
