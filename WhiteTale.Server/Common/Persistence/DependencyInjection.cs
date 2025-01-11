using System.ComponentModel.DataAnnotations;

namespace WhiteTale.Server.Common.Persistence;

internal static class DependencyInjection
{
	internal static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
	{
		_ = services.AddOptions<PersistenceOptions>()
			.BindConfiguration(PersistenceOptions.SectionName)
			.ValidateDataAnnotations()
			.ValidateOnStart();

		var settings = new PersistenceOptions();
		configuration.GetSection(PersistenceOptions.SectionName).Bind(settings);
		Validator.ValidateObject(settings, new ValidationContext(settings));

		_ = services.AddDbContext<ApplicationDbContext>(builder =>
		{
			_ = settings.Provider switch
			{
				DatabaseProvider.InMemory => builder.UseInMemoryDatabase(nameof(DatabaseProvider.InMemory)),
				DatabaseProvider.Sqlite or _ => builder.UseSqlite(settings.ConnectionString)
			};
		});

		return services;
	}
}
