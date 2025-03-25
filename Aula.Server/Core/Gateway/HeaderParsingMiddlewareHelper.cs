using Microsoft.AspNetCore.Builder;

namespace Aula.Server.Core.Gateway;

internal static class HeaderParsingMiddlewareHelper
{
	internal static TBuilder UseWebSocketHeaderParsing<TBuilder>(this TBuilder builder)
		where TBuilder : IApplicationBuilder
	{
		_ = builder.UseMiddleware<HeaderParsingMiddleware>();

		return builder;
	}
}
