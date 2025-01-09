namespace WhiteTale.Server.Configuration;

internal static class ConfigurationExtensions
{
	public static T GetRequiredValue<T>(this IConfiguration configuration, String key) where T : notnull
	{
		ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));
		ArgumentNullException.ThrowIfNull(key, nameof(key));

		var typeAsNullable = typeof(T).IsValueType ? typeof(Nullable<>).MakeGenericType(typeof(T)) : typeof(T);
		return (T?)configuration.GetValue(typeAsNullable, key) ??
		       throw new KeyNotFoundException($"{key} was not found in the configuration.");
	}
}
