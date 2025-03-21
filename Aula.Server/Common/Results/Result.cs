using System.Diagnostics.CodeAnalysis;

namespace Aula.Server.Common.Results;

internal readonly struct Result : IEquatable<Result>
{
	internal static Result Success => new();

	internal ResultProblemValues Problems { get; }

	internal Boolean Succeeded => Problems.Count is 0;

	internal Result(ResultProblemValues problems)
	{
		Problems = problems;
	}

	public Boolean Equals(Result other)
	{
		return Problems.Equals(other.Problems);
	}

	public override Boolean Equals(Object? obj)
	{
		return obj is Result other && Equals(other);
	}

	public override Int32 GetHashCode()
	{
		return Problems.GetHashCode();
	}

	public static Boolean operator ==(Result left, Result right) => left.Equals(right);

	public static Boolean operator !=(Result left, Result right) => !left.Equals(right);
}

internal readonly struct Result<TResult> : IEquatable<Result<TResult>>
{
	internal TResult? Value { get; }

	internal ResultProblemValues Problems { get; }

	internal Boolean Succeeded => Problems.Count is 0;

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

	public Boolean Equals(Result<TResult> other)
	{
		return Succeeded == other.Succeeded &&
		       Problems.Equals(other.Problems) &&
		       ((Value is not null && Value.Equals(other.Value)) ||
		        (Value is null && other.Value is null));
	}

	public override Boolean Equals(Object? obj)
	{
		return obj is Result<TResult> other && Equals(other);
	}

	public override Int32 GetHashCode()
	{
		return HashCode.Combine(Succeeded, Value, Problems);
	}

	public static Boolean operator ==(Result<TResult> left, Result<TResult> right) => left.Equals(right);

	public static Boolean operator !=(Result<TResult> left, Result<TResult> right) => !left.Equals(right);
}
