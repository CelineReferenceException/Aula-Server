namespace Aula.Server.Core.Domain;

internal interface IDomainEntity
{
	internal IReadOnlyList<DomainEvent> Events { get; }

	internal void ClearEvents();
}
