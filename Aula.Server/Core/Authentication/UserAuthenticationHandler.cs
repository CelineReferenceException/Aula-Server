using System.Security.Claims;
using System.Text.Encodings.Web;
using Aula.Server.Core.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Aula.Server.Core.Authentication;

/// <summary>
///     Authenticate clients that provide a bearer token.
/// </summary>
internal sealed class UserAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
	private readonly TokenProvider _tokenProvider;

	public UserAuthenticationHandler(
		IOptionsMonitor<AuthenticationSchemeOptions> options,
		ILoggerFactory logger,
		UrlEncoder encoder,
		TokenProvider tokenProvider) : base(options, logger, encoder)
	{
		_tokenProvider = tokenProvider;
	}

	protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
	{
		if (!Context.Request.Headers.TryGetValue("Authorization", out var headerValues))
		{
			return AuthenticateResult.NoResult();
		}

		var headerValue = headerValues.FirstOrDefault().AsSpan();

		var headerValueSegments = headerValue.Split(' ');
		if (!headerValueSegments.MoveNext() ||
		    !headerValueSegments.MoveNext())
		{
			return AuthenticateResult.NoResult();
		}

		var tokenSegmentStart = headerValueSegments.Current.Start.Value;
		var tokenSegmentLength = headerValueSegments.Current.End.Value - tokenSegmentStart;
		var tokenSegment = headerValue.Slice(tokenSegmentStart, tokenSegmentLength);

		if (tokenSegment.IsEmpty)
		{
			return AuthenticateResult.NoResult();
		}

		if (!_tokenProvider.TryReadFromToken(tokenSegment, out var userIdString, out var securityStamp) ||
		    !Snowflake.TryParse(userIdString, out var userId))
		{
			return AuthenticateResult.NoResult();
		}

		var userManager = Context.RequestServices.GetRequiredService<UserManager>();
		var user = await userManager.FindByIdAsync(userId);
		if (user is null ||
		    user.SecurityStamp != securityStamp)
		{
			return AuthenticateResult.NoResult();
		}

		var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userIdString, ClaimValueTypes.UInteger64), };
		var claimsIdentity = new ClaimsIdentity(claims, AuthenticationSchemes.BearerToken);
		var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
		return AuthenticateResult.Success(new AuthenticationTicket(claimsPrincipal, AuthenticationSchemes.BearerToken));
	}
}
