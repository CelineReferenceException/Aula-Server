using System.Diagnostics.CodeAnalysis;

namespace WhiteTale.Server.Common.Results;

internal class Result
{
	private protected Result()
	{
		Succeeded = true;
	}

	private protected Result(ResultError error)
	{
		ArgumentNullException.ThrowIfNull(error);
		Error = error;
	}

	public static Result Success { get; } = new();

	[MemberNotNullWhen(false, nameof(Error))]
	public bool Succeeded { get; }

	public ResultError? Error { get; }

	[SuppressMessage("Design", "CA1062:Validate arguments of public methods")]
	[SuppressMessage("Usage", "CA2225:Operator overloads have named alternates")]
	public static explicit operator ResultError(Result result) =>
		result.Error ?? throw new InvalidCastException("Result has no error");

	[SuppressMessage("Design", "CA1062:Validate arguments of public methods")]
	[SuppressMessage("Usage", "CA2225:Operator overloads have named alternates")]
	public static implicit operator Result(ResultError error) => new(error);

	public TReturn Match<TReturn>(Func<TReturn> onSuccess, Func<ResultError, TReturn> onFailure)
	{
		ArgumentNullException.ThrowIfNull(onSuccess);
		ArgumentNullException.ThrowIfNull(onFailure);

		return Error is not null ? onFailure.Invoke(Error) : onSuccess();
	}
}

internal sealed class Result<TValue> : Result
{
	private Result(TValue value)
	{
		Value = value;
	}

	private Result(ResultError error) : base(error)
	{
	}

	[MemberNotNullWhen(true, nameof(Value))]
	[MemberNotNullWhen(false, nameof(Error))]
	public new bool Succeeded => base.Succeeded;

	public new ResultError? Error => base.Error;

	public TValue? Value { get; }

	[SuppressMessage("Design", "CA1062:Validate arguments of public methods")]
	[SuppressMessage("Usage", "CA2225:Operator overloads have named alternates")]
	public static explicit operator TValue(Result<TValue> result)
		=> result.Value ?? throw new InvalidCastException("Result has no value");

	[SuppressMessage("Design", "CA1062:Validate arguments of public methods")]
	[SuppressMessage("Usage", "CA2225:Operator overloads have named alternates")]
	public static explicit operator ResultError(Result<TValue> result) =>
		result.Error ?? throw new InvalidCastException("Result has no exception");

	[SuppressMessage("Usage", "CA2225:Operator overloads have named alternates")]
	public static implicit operator Result<TValue>(TValue value) => new(value);

	[SuppressMessage("Usage", "CA2225:Operator overloads have named alternates")]
	public static implicit operator Result<TValue>(ResultError exception) => new(exception);

	public TReturn Match<TReturn>(Func<TValue?, TReturn> onSuccess, Func<ResultError, TReturn> onFailure)
	{
		ArgumentNullException.ThrowIfNull(onSuccess);
		ArgumentNullException.ThrowIfNull(onFailure);

		return Error is not null ? onFailure(Error) : onSuccess(Value);
	}
}
