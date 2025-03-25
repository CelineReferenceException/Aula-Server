using System.Buffers.Text;
using System.Text;
using Aula.Server.Core.Domain.Users;
using Aula.Server.Core.Endpoints;
using Aula.Server.Core.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace Aula.Server.Core.Features.Identity;

internal sealed class ConfirmEmailEndpoint : IEndpoint
{
	internal const String EmailQueryParameter = "email";
	internal const String TokenQueryParameter = "token";

	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapPost("identity/confirm-email", HandleAsync)
			.HasApiVersion(1);
	}

	private static async Task<Results<NoContent, RedirectHttpResult, ProblemHttpResult>> HandleAsync(
		[FromQuery(Name = EmailQueryParameter)] String email,
		[FromQuery(Name = TokenQueryParameter)] String? token,
		[FromServices] UserManager userManager,
		[FromServices] ConfirmEmailEmailSender confirmEmailEmailSender,
		[FromServices] IOptions<IdentityFeatureOptions> featureOptions)
	{
		if (!Base64Url.IsValid(email))
		{
			return TypedResults.Problem(ProblemDetailsDefaults.InvalidBase64UrlEmail);
		}

		email = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(email));
		var redirectUri = featureOptions.Value.ConfirmEmailRedirectUri?.ToString();

		var user = await userManager.FindByEmailAsync(email);
		if (user?.Type is not UserType.Standard)
		{
			// We could return NotFound, but it feels like unnecessary information.
			return RedirectOrSendNoContent(redirectUri);
		}

		if (user.EmailConfirmed)
		{
			return RedirectOrSendNoContent(redirectUri);
		}

		if (token is null)
		{
			await confirmEmailEmailSender.SendEmailAsync(user);
			return RedirectOrSendNoContent(redirectUri);
		}

		try
		{
			token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
		}
		catch (FormatException)
		{
			return RedirectOrSendNoContent(redirectUri);
		}

		_ = await userManager.ConfirmEmailAsync(user, token);

		return RedirectOrSendNoContent(redirectUri);
	}

	private static Results<NoContent, RedirectHttpResult, ProblemHttpResult> RedirectOrSendNoContent(String? redirectUri)
	{
		return redirectUri is not null ? TypedResults.Redirect(redirectUri) : TypedResults.NoContent();
	}
}
