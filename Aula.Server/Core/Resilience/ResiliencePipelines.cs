namespace Aula.Server.Core.Resilience;

internal static class ResiliencePipelines
{
	private const String Prefix = nameof(ResiliencePipelines);

	internal const String RetryOnDbConcurrencyProblem = $"{Prefix}.{nameof(RetryOnDbConcurrencyProblem)}";
}
