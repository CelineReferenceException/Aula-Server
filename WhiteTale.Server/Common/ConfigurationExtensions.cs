using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace WhiteTale.Server.Common;

internal static class ConfigurationExtensions
{
	private const string SectionName = "WhiteTale";
	private static WhiteTaleSettings? s_settings;

	public static WhiteTaleSettings GetWhiteTaleSettings(this IConfiguration configuration)
	{
		return s_settings ?? throw new InvalidOperationException("Configuration has not been initialized.");
	}

	public static void UseWhiteTaleSettings(this IHost host)
	{
		IConfigurationSection section = host.Services.GetRequiredService<IConfiguration>().GetSection(SectionName);
		if (!section.Exists())
		{
			throw new InvalidOperationException($"Missing configuration section '{SectionName}'");
		}

		s_settings = new WhiteTaleSettings();
	}
}
