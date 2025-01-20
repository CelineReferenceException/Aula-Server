namespace WhiteTale.Server.Domain;

internal interface IDomainEntity
{
	IReadOnlyList<DomainEvent> DomainEvents { get; }
}
