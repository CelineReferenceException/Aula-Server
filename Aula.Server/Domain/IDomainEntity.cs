namespace Aula.Server.Domain;

internal interface IDomainEntity
{
	internal IReadOnlyList<DomainEvent> Events { get; }
}
