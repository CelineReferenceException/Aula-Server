using System.ComponentModel.DataAnnotations;

namespace Aula.Server.Common.Cache;

internal static class DependencyInjection
{
	internal static IServiceCollection AddCache(this IServiceCollection services, IConfiguration configuration)
	{
		_ = services.AddOptions<CacheOptions>()
			.BindConfiguration(CacheOptions.SectionName)
			.ValidateDataAnnotations()
			.ValidateOnStart();

		var settings = new CacheOptions();
		configuration.GetSection(CacheOptions.SectionName).Bind(settings);
		Validator.ValidateObject(settings, new ValidationContext(settings));

		_ = services.AddStackExchangeRedisCache(options =>
		{
			options.Configuration = settings.ConnectionString;
			options.InstanceName = settings.InstanceName;
		});

		return services;
	}
}
