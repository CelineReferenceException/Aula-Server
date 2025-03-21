using System.Globalization;
using System.Text.RegularExpressions;

namespace Aula.Server.Common;

internal static partial class EmailAddressValidator
{
	private static readonly IdnMapping s_idn = new();

	[GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase, 250)]
	private static partial Regex EmailRegex { get; }

	[GeneratedRegex(@"(@)(.+)$")]
	private static partial Regex DomainRegex { get; }

	internal static Boolean IsValid(String email)
	{
		if (String.IsNullOrWhiteSpace(email))
		{
			return false;
		}

		try
		{
			email = DomainRegex.Replace(email, DomainMapper);

			static String DomainMapper(Match match)
			{
				var domainName = s_idn.GetAscii(match.Groups[2].Value);
				return match.Groups[1].Value + domainName;
			}
		}
		catch (RegexMatchTimeoutException e)
		{
			return false;
		}
		catch (ArgumentException e)
		{
			return false;
		}

		try
		{
			return EmailRegex.IsMatch(email);
		}
		catch (RegexMatchTimeoutException)
		{
			return false;
		}
	}
}
