using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Aula.Server.Common.Persistence;

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
			_ = builder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

			_ = settings.Provider switch
			{
				PersistenceProvider.InMemory => builder.UseInMemoryDatabase(nameof(PersistenceProvider.InMemory)),
				PersistenceProvider.Sqlite => builder.UseSqlite(settings.ConnectionString),
				_ => throw new NotImplementedException(),
			};
		}, ServiceLifetime.Transient);

		return services;
	}
}
