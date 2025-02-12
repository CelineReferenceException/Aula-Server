using System.Text.Json;
using Aula.Server.Common.Gateway;
using Aula.Server.Common.Persistence;
using Aula.Server.Domain.Users;
using MediatR;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Aula.Server.Features.Messages.Gateway;

internal sealed class UserStartedTypingEventHandler : INotificationHandler<UserStartedTypingEvent>
{
	private readonly ApplicationDbContext _dbContext;
	private readonly GatewayService _gatewayService;
	private readonly JsonSerializerOptions _jsonSerializerOptions;

	public UserStartedTypingEventHandler(IOptions<JsonOptions> jsonOptions, ApplicationDbContext dbContext, GatewayService gatewayService)
	{
		_jsonSerializerOptions = jsonOptions.Value.SerializerOptions;
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

			var user = sessionUsers[session.UserId];
			if ((user.CurrentRoomId is null ||
			     user.CurrentRoomId != notification.RoomId) &&
			    !user.Permissions.HasFlag(Permissions.Administrator))
			{
				continue;
			}

			_ = session.QueueEventAsync(payload, cancellationToken);
		}
	}
}
