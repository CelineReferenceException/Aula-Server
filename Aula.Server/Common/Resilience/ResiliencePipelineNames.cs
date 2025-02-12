namespace Aula.Server.Common.Resilience;

internal static class ResiliencePipelineNames
{
	private const String Prefix = nameof(ResiliencePipelineNames);

	internal const String RetryOnDbConcurrencyProblem = $"{Prefix}.{nameof(RetryOnDbConcurrencyProblem)}";
}
