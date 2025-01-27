using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

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
				PersistenceProvider.InMemory => builder.UseInMemoryDatabase(nameof(PersistenceProvider.InMemory)),
				PersistenceProvider.Sqlite or _ => builder.UseSqlite(settings.ConnectionString),
			};
		}, ServiceLifetime.Transient);

		return services;
	}
}
