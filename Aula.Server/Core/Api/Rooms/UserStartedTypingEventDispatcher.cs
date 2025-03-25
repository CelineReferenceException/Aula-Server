using Aula.Server.Common.Persistence;
using Aula.Server.Domain.Users;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Aula.Server.Core.Api.Rooms;

internal sealed class UserStartedTypingEventDispatcher : INotificationHandler<UserStartedTypingEvent>
{
	private readonly ApplicationDbContext _dbContext;
	private readonly GatewayService _gatewayService;

	public UserStartedTypingEventDispatcher(ApplicationDbContext dbContext, GatewayService gatewayService)
	{
		_dbContext = dbContext;
		_gatewayService = gatewayService;
	}

	public async Task Handle(UserStartedTypingEvent notification, CancellationToken cancellationToken)
	{
		var payload = new GatewayPayload<UserTypingEventData>
		{
			Operation = OperationType.Dispatch,
			Event = EventType.UserStartedTyping,
			Data = new UserTypingEventData
			{
				UserId = notification.UserId,
				RoomId = notification.RoomId,
			},
		};

		var sessionUserIds = _gatewayService.Sessions.Values
			.Select(session => session.UserId);

		var sessionUsers = await _dbContext.Users
			.Where(u => sessionUserIds.Contains(u.Id))
			.Select(u => new
			{
				u.Id,
				u.CurrentRoomId,
				u.Permissions,
			})
			.ToDictionaryAsync(u => u.Id, u => new
			{
				u.CurrentRoomId,
				u.Permissions,
			}, cancellationToken);

		foreach (var session in _gatewayService.Sessions.Values)
		{
			if (!session.Intents.HasFlag(Intents.Messages))
			{
				continue;
			}

			if (!sessionUsers.TryGetValue(session.UserId, out var user) ||
			    (user.CurrentRoomId != notification.RoomId &&
			     !user.Permissions.HasFlag(Permissions.Administrator)))
			{
				continue;
			}

			_ = session.QueueEventAsync(payload, cancellationToken);
		}
	}
}
