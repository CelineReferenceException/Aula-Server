namespace Aula.Server.Common.Results;

internal readonly ref struct Result<TProblem> : IEquatable<Result<TProblem>>
	where TProblem : class?
{
	internal Result(ResultErrorValues<TProblem> errors)
	{
		Errors = errors;
	}

	internal static Result<TProblem> Success => default;

	internal ResultErrorValues<TProblem> Errors { get; }

	internal Boolean Succeeded => Errors.Count is 0;

	public static implicit operator Result<TProblem>(ResultErrorValues<TProblem> errors) => new(errors);

	public static Boolean operator ==(Result<TProblem> left, Result<TProblem> right) => left.Equals(right);

	public static Boolean operator !=(Result<TProblem> left, Result<TProblem> right) => !left.Equals(right);

	public Boolean Equals(Result<TProblem> other)
	{
		return Errors.Equals(other.Errors);
	}

	public override Boolean Equals(Object? obj)
	{
		return false;
	}

	public override Int32 GetHashCode()
	{
		return Errors.GetHashCode();
	}
}
