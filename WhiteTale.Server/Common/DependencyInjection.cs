namespace WhiteTale.Server.Common;

internal static class DependencyInjection
{
	internal static TBuilder AddCommon<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
	{
		_ = builder.Services.AddEndpoints();

		return builder;
	}

	internal static TApp UseCommon<TApp>(this TApp app) where TApp : IApplicationBuilder, IEndpointRouteBuilder
	{
		_ = app.MapEndpoints();

		return app;
	}
}
