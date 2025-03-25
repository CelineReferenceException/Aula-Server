namespace Aula.Server.Common.Results;

internal sealed class ResultFailureException : Exception
{
	private const String DefaultMessage = "The operation did not succeed.";

	public ResultFailureException()
		: base(DefaultMessage)
	{
	}

	public ResultFailureException(String? message)
		: base(message ?? DefaultMessage)
	{
	}

	public ResultFailureException(String? message, Exception? innerException)
		: base(message ?? DefaultMessage, innerException)
	{
	}
}
