namespace Aula.Server.Core.Domain.Rooms;

internal sealed record RoomConnectionRemovedEvent(RoomConnection Connection) : DomainEvent;
