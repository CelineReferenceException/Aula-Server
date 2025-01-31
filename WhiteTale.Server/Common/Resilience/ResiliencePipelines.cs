using Polly;

namespace WhiteTale.Server.Common.Resilience;

internal sealed class ResiliencePipelines
{
	internal ResiliencePipeline RetryOnDbConcurrencyProblem { get; private set; }

	public ResiliencePipelines(
		[FromKeyedServices(ResiliencePipelineNames.RetryOnDbConcurrencyProblem)] ResiliencePipeline retryOnDbConcurrencyProblem)
	{
		RetryOnDbConcurrencyProblem = retryOnDbConcurrencyProblem;
	}
}
