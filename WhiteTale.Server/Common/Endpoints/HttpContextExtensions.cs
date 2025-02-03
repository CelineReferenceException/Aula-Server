using System.Text;
using Microsoft.AspNetCore.Http;

namespace WhiteTale.Server.Common.Endpoints;

internal static class HttpContextExtensions
{
	private const String SchemeDelimiter = "://";

	internal static String GetUrl(this HttpRequest request)
	{
		var scheme = request.Scheme;
		var host = request.Host.Value ?? String.Empty;
		var pathBase = request.PathBase.Value ?? String.Empty;
		var path = request.Path.Value ?? String.Empty;

		var length = scheme.Length + SchemeDelimiter.Length + host.Length
		             + pathBase.Length + path.Length;

		return new StringBuilder(length)
			.Append(scheme)
			.Append(SchemeDelimiter)
			.Append(host)
			.Append(pathBase)
			.Append(path)
			.ToString();
	}
}
