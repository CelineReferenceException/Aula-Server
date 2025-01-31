using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace WhiteTale.Server.Features.Bans;

internal sealed class RemoveIpAddressBan : IEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapDelete("bans/ip-address/{ipAddress}", HandleAsync)
			.RequireRateLimiting(RateLimitPolicyNames.Global)
			.RequireAuthorization(IdentityAuthorizationPolicyNames.BearerToken)
			.RequirePermissions(Permissions.BanUsers)
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Results<NoContent, ProblemHttpResult, InternalServerError>> HandleAsync(
		[FromRoute] String ipAddress,
		[FromServices] IpAddressValidator ipAddressValidator,
		[FromServices] ApplicationDbContext dbContext)
	{
		var ipValidation = await ipAddressValidator.ValidateAsync(ipAddress);
		if (!ipValidation.IsValid)
		{
			var problemDetails = ipValidation.Errors.ToProblemDetails();
			return TypedResults.Problem(problemDetails);
		}

		var ban = await dbContext.Bans
			.AsTracking()
			.Where(x => x.IpAddress == ipAddress)
			.FirstOrDefaultAsync();
		if (ban is null)
		{
			return TypedResults.NoContent();
		}

		ban.Remove();
		_ = dbContext.Bans.Remove(ban);

		try
		{
			_ = await dbContext.SaveChangesAsync();
		}
		catch (DbUpdateConcurrencyException)
		{
			return TypedResults.InternalServerError();
		}

		return TypedResults.NoContent();
	}
}
