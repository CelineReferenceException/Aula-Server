namespace Aula.Server.Common.Results;

internal readonly ref struct Result<TResult, TError>
	where TError : class?
{
	private readonly TResult? _value;

	internal Result(TResult value)
	{
		_value = value;
	}

	internal Result(ResultErrorValues<TError> errors)
	{
		if (errors.Count < 1)
		{
			throw new ArgumentException($"{nameof(errors)} cannot be empty.", nameof(errors));
		}

		Errors = errors;
	}

	internal TResult? Value => Succeeded ? _value : throw new InvalidOperationException("Result did not succeed.");

	internal ResultErrorValues<TError> Errors { get; }

	internal Boolean Succeeded => Errors.Count is 0;

	public static implicit operator Result<TResult, TError>(TResult value) => new(value);

	public static implicit operator Result<TResult, TError>(ResultErrorValues<TError> errorValues) => new(errorValues);

	public static Boolean operator ==(Result<TResult, TError> left, Result<TResult, TError> right) => left.Equals(right);

	public static Boolean operator !=(Result<TResult, TError> left, Result<TResult, TError> right) => !left.Equals(right);

	public Boolean Equals(Result<TResult, TError> other)
	{
		return Succeeded == other.Succeeded &&
		       Errors.Equals(other.Errors) &&
		       ((Value is not null && Value.Equals(other.Value)) ||
		        (Value is null && other.Value is null));
	}

	public override Boolean Equals(Object? obj)
	{
		return false;
	}

	public override Int32 GetHashCode()
	{
		return HashCode.Combine(Value, Errors.GetHashCode());
	}
}
