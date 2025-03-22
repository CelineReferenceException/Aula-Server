namespace Aula.Server.Common.Results;

internal readonly ref struct Result<TResult>
{
	internal Result(TResult value)
	{
		Value = value;
	}

	internal Result(ResultProblemValues problems)
	{
		if (problems.Count < 1)
		{
			throw new ArgumentException($"{nameof(problems)} cannot be empty.", nameof(problems));
		}

		Problems = problems;
	}

	internal TResult? Value { get; }

	internal ResultProblemValues Problems { get; }

	internal Boolean Succeeded => Problems.Count is 0;

	public static Boolean operator ==(Result<TResult> left, Result<TResult> right) => left.Equals(right);

	public static Boolean operator !=(Result<TResult> left, Result<TResult> right) => !left.Equals(right);

	public Boolean Equals(Result<TResult> other)
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
