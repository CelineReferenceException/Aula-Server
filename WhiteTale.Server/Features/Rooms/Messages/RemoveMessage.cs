using WhiteTale.Server.Domain.Users;

namespace WhiteTale.Server.Features.Rooms.Messages;

internal sealed class RemoveMessage : IEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapDelete("api/rooms/{roomId}/messages/{messageId}", HandleAsync)
			.RequireRateLimiting(CommonRateLimitPolicyNames.Global)
			.RequireAuthorization(IdentityAuthorizationPolicyNames.BearerToken);
	}

	private static async Task<Results<Ok<MessageData>, NotFound, ForbidHttpResult, ProblemHttpResult, InternalServerError>> HandleAsync(
		[FromRoute] UInt64 roomId,
		[FromRoute] UInt64 messageId,
		[FromServices] ApplicationDbContext dbContext,
		HttpContext httpContext,
		[FromServices] UserManager<User> userManager)
	{
		var roomExists = await dbContext.Rooms
			.AsNoTracking()
			.AnyAsync(room => room.Id == roomId && !room.IsRemoved);
		if (!roomExists)
		{
			return TypedResults.Problem(new ProblemDetails
			{
				Title = "Invalid room ID",
				Detail = "The room does not exist",
				Status = StatusCodes.Status400BadRequest
			});
		}

		var message = await dbContext.Messages
			.AsTracking()
			.Where(message => message.Id == messageId && !message.IsRemoved)
			.FirstOrDefaultAsync();
		if (message is null)
		{
			return TypedResults.NotFound();
		}

		var user = await userManager.GetUserAsync(httpContext.User);
		if (user is null)
		{
			return TypedResults.InternalServerError();
		}

		if (message.AuthorId != user.Id &&
		    !user.Permissions.HasAnyFlag(Permissions.Administrator | Permissions.ManageMessages))
		{
			return TypedResults.Forbid();
		}

		message.Remove();

		try
		{
			_ = await dbContext.SaveChangesAsync();
		}
		catch (DbUpdateConcurrencyException)
		{
			return TypedResults.InternalServerError();
		}

		return TypedResults.Ok(new MessageData
		{
			Id = message.Id,
			Type = message.Type,
			Flags = message.Flags,
			AuthorId = message.AuthorId,
			Target = message.Target,
			TargetId = message.TargetId,
			Content = message.Content,
			CreationTime = message.CreationTime
		});
	}
}
