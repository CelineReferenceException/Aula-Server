using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace WhiteTale.Server.Common.Authentication;

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
		var userManager = Context.RequestServices.GetRequiredService<UserManager>();

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
		var tokenSegmentLength = tokenSegmentStart - headerValueSegments.Current.End.Value;
		var tokenSegment = headerValue.Slice(tokenSegmentStart, tokenSegmentLength);


		if (tokenSegment.IsEmpty)
		{
			return AuthenticateResult.NoResult();
		}

		if (!_tokenProvider.TryReadFromToken(headerValue, out var userId, out var securityStamp))
		{
			return AuthenticateResult.NoResult();
		}

		var user = await userManager.FindByIdAsync((UInt64)userId);
		if (user is null ||
		    user.SecurityStamp != securityStamp)
		{
			return AuthenticateResult.NoResult();
		}

		var claims = new[] { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString(), ClaimValueTypes.UInteger64), };
		var claimsIdentity = new ClaimsIdentity(claims);
		var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
		return AuthenticateResult.Success(new AuthenticationTicket(claimsPrincipal, AuthenticationSchemeNames.BearerToken));
	}
}
