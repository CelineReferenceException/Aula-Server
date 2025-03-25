namespace Aula.Server.Domain;

internal abstract class DefaultDomainEntity : IDomainEntity
{
	IReadOnlyList<DomainEvent> IDomainEntity.Events => Events;

	private protected List<DomainEvent> Events { get; } = [];

	void IDomainEntity.ClearEvents()
	{
		Events.Clear();
	}
}
