using System.Text.Json;
using Aula.Server.Core.Domain.Users;
using Aula.Server.Core.Gateway;
using Aula.Server.Core.Json;
using Aula.Server.Core.Persistence;
using MediatR;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Aula.Server.Core.Features.Rooms;

internal sealed class UserStoppedTypingEventDispatcher : INotificationHandler<UserStoppedTypingEvent>
{
	private readonly ApplicationDbContext _dbContext;
	private readonly GatewayService _gatewayService;
	private readonly JsonSerializerOptions _jsonSerializerOptions;

	public UserStoppedTypingEventDispatcher(IOptions<JsonOptions> jsonOptions, ApplicationDbContext dbContext, GatewayService gatewayService)
	{
		_jsonSerializerOptions = jsonOptions.Value.SerializerOptions;
		_dbContext = dbContext;
		_gatewayService = gatewayService;
	}


	public async Task Handle(UserStoppedTypingEvent notification, CancellationToken cancellationToken)
	{
		var payload = new GatewayPayload<UserTypingEventData>
		{
			Operation = OperationType.Dispatch,
			Event = EventType.UserStoppedTyping,
			Data = new UserTypingEventData
			{
				UserId = notification.UserId,
				RoomId = notification.RoomId,
			},
		}.GetJsonUtf8Bytes(_jsonSerializerOptions);

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
