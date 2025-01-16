using MediatR;

namespace WhiteTale.Server.Domain;

internal abstract record DomainEvent : INotification;
