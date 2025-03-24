namespace Aula.Server.Common;

internal static class StringExtensions
{
	internal static String ToCamel(this ReadOnlySpan<Char> span)
	{
		return String.Create(span.Length, span, static (newSpan, span) =>
		{
			span.CopyTo(newSpan);
			newSpan[0] = Char.ToLowerInvariant(span[0]);
		});
	}

	internal static String ToCamel(this String str)
	{
		return str.AsSpan().ToCamel();
	}
}
