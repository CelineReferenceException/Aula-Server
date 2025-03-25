namespace Aula.Server.Common.Resilience;

internal static class ResiliencePipelines
{
	internal const String RetryOnDbConcurrencyProblem = $"{Prefix}.{nameof(RetryOnDbConcurrencyProblem)}";
	private const String Prefix = nameof(ResiliencePipelines);
}
