using Polly;

namespace WhiteTale.Server.Common.Resilience;

internal sealed class ResiliencePipelines
{
	public ResiliencePipelines(
		[FromKeyedServices(ResiliencePipelineNames.RetryOnDbConcurrencyProblem)] ResiliencePipeline retryOnDbConcurrencyProblem)
	{
		RetryOnDbConcurrencyProblem = retryOnDbConcurrencyProblem;
	}

	internal ResiliencePipeline RetryOnDbConcurrencyProblem { get; }
}
