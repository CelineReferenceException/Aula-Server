using MediatR;

namespace Aula.Server.Core.Domain;

internal abstract record DomainEvent : INotification;
