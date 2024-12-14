namespace WhiteTale.Server.Common.Results;

internal sealed class InvalidOperationError : ResultError
{
	internal InvalidOperationError(string name) : base(name)
	{
	}
}
