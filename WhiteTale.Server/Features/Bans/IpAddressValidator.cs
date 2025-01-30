using System.Net;
using FluentValidation;

namespace WhiteTale.Server.Features.Bans;

internal sealed class IpAddressValidator : AbstractValidator<String>
{
	public IpAddressValidator()
	{
		_ = RuleFor(x => x)
			.Must(x => IPAddress.TryParse(x, out _))
			.WithErrorCode("Invalid IP Address")
			.WithMessage("The IP address is not valid.");
	}
}
