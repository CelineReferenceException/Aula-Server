namespace Aula.Server.Common.Results;

internal readonly ref struct Result<TResult, TProblem>
	where TProblem : class?
{
	internal Result(TResult value)
	{
		Value = value;
	}

	internal Result(ResultProblemValues<TProblem> problems)
	{
		if (problems.Count < 1)
		{
			throw new ArgumentException($"{nameof(problems)} cannot be empty.", nameof(problems));
		}

		Problems = problems;
	}

	internal TResult? Value { get; }

	internal ResultProblemValues<TProblem> Problems { get; }

	internal Boolean Succeeded => Problems.Count is 0;

	public static implicit operator Result<TResult, TProblem>(TResult value) => new(value);

	public static implicit operator Result<TResult, TProblem>(ResultProblemValues<TProblem> problemValues) => new(problemValues);

	public static Boolean operator ==(Result<TResult, TProblem> left, Result<TResult, TProblem> right) => left.Equals(right);

	public static Boolean operator !=(Result<TResult, TProblem> left, Result<TResult, TProblem> right) => !left.Equals(right);

	public Boolean Equals(Result<TResult, TProblem> other)
	{
		return Succeeded == other.Succeeded &&
		       Problems.Equals(other.Problems) &&
		       ((Value is not null && Value.Equals(other.Value)) ||
		        (Value is null && other.Value is null));
	}

	public override Boolean Equals(Object? obj)
	{
		return false;
	}

	public override Int32 GetHashCode()
	{
		return HashCode.Combine(Value, Problems.GetHashCode());
	}
}
