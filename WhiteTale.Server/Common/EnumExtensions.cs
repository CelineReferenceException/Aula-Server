namespace WhiteTale.Server.Common;

internal static class EnumExtensions
{
	internal static IEnumerable<TEnum> GetFlags<TEnum>(this TEnum e) where TEnum : struct, Enum
	{
		var allFlags = Enum.GetValues<TEnum>();
		return allFlags.Where(flag => e.HasFlag(flag));
	}
}
