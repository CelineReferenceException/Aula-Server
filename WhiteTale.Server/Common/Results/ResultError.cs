namespace WhiteTale.Server.Common.Results;

internal abstract class ResultError
{
	private protected ResultError(string? message = null)
	{
		Message = message;
	}

	public string? Message { get; }
}
