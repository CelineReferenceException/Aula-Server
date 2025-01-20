namespace WhiteTale.Server.Domain;

internal interface IDomainEntity
{
	IReadOnlyList<DomainEvent> Events { get; }
}
