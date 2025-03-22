namespace Aula.Server.Common.Results;

internal readonly ref struct Result : IEquatable<Result>
{
	internal Result(ResultProblemValues problems)
	{
		Problems = problems;
	}

	internal static Result Success => default;

	internal ResultProblemValues Problems { get; }

	internal Boolean Succeeded => Problems.Count is 0;

	public static implicit operator Result(ResultProblemValues problems) => new(problems);

	public static Boolean operator ==(Result left, Result right) => left.Equals(right);

	public static Boolean operator !=(Result left, Result right) => !left.Equals(right);

	public Boolean Equals(Result other)
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
