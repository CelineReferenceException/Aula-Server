namespace Aula.Server.Common.Results;

internal readonly ref struct Result<TProblem> : IEquatable<Result<TProblem>>
	where TProblem : class?
{
	internal Result(ResultProblemValues<TProblem> problems)
	{
		Problems = problems;
	}

	internal static Result<TProblem> Success => default;

	internal ResultProblemValues<TProblem> Problems { get; }

	internal Boolean Succeeded => Problems.Count is 0;

	public static implicit operator Result<TProblem>(ResultProblemValues<TProblem> problems) => new(problems);

	public static Boolean operator ==(Result<TProblem> left, Result<TProblem> right) => left.Equals(right);

	public static Boolean operator !=(Result<TProblem> left, Result<TProblem> right) => !left.Equals(right);

	public Boolean Equals(Result<TProblem> other)
	{
		return Problems.Equals(other.Problems);
	}

	public override Boolean Equals(Object? obj)
	{
		return false;
	}

	public override Int32 GetHashCode()
	{
		return Problems.GetHashCode();
	}
}
