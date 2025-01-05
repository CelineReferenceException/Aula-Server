namespace WhiteTale.Server.Common.Persistence;

internal static class DependencyInjection
{
	internal static IServiceCollection AddPersistence(this IServiceCollection services)
	{
		_ = services.AddOptions<PersistenceOptions>()
			.BindConfiguration(PersistenceOptions.SectionName)
			.ValidateDataAnnotations()
			.ValidateOnStart();

		_ = services.AddDbContext<ApplicationDbContext>();

		return services;
	}
}
