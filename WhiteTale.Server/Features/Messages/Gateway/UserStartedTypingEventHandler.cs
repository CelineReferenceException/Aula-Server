using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace WhiteTale.Server.Features.Messages.Gateway;

internal sealed class UserStartedTypingEventHandler : INotificationHandler<UserStartedTypingEvent>
{
	private readonly ApplicationDbContext _dbContext;
	private readonly JsonSerializerOptions _jsonSerializerOptions;
	private readonly GatewayService _gatewayService;

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
		};
		var payloadBytes = JsonSerializer.SerializeToUtf8Bytes(payload, _jsonSerializerOptions);

		var sessionUserIds = _gatewayService.Sessions.Values
			.Select(session => session.UserId);

		var usersCurrentRoomId = await _dbContext.Users
			.Where(u => sessionUserIds.Contains(u.Id))
			.Select(u => new
			{
				u.Id,
				u.CurrentRoomId,
			})
			.ToDictionaryAsync(u => u.Id, u => u.CurrentRoomId, cancellationToken);

		foreach (var session in _gatewayService.Sessions.Values)
		{
			if (!session.Intents.HasFlag(Intents.Messages))
			{
				continue;
			}

			var userCurrentRoomId = usersCurrentRoomId[session.UserId];
			if (userCurrentRoomId is null ||
			    userCurrentRoomId != notification.RoomId)
			{
				continue;
			}

			_ = session.QueueEventAsync(payloadBytes, cancellationToken);
		}
	}
}
