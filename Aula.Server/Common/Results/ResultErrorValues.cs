namespace Aula.Server.Common.Results;

internal readonly ref struct ResultErrorValues<TError> : IReadOnlyList<TError>, IEquatable<ResultErrorValues<TError>>
	where TError : class?
{
	private readonly Items<TError> _values;

	internal ResultErrorValues(Items<TError> values)
	{
		_values = values;
	}

	public Int32 Count => _values.Count;

	internal static ResultErrorValues<TError> Empty => new();

	public TError this[Int32 index]
	{
		get => _values[index];
		set => throw new NotSupportedException();
	}

	public static Boolean operator ==(ResultErrorValues<TError> left, ResultErrorValues<TError> right) => left.Equals(right);

	public static Boolean operator !=(ResultErrorValues<TError> left, ResultErrorValues<TError> right) => !left.Equals(right);

	public Items<TError>.Enumerator GetEnumerator()
	{
		return _values.GetEnumerator();
	}

	public Boolean Equals(ResultErrorValues<TError> other)
	{
		return _values.Equals(other._values);
	}

	public override Boolean Equals(Object? obj)
	{
		return false;
	}

	public override Int32 GetHashCode()
	{
		return _values.GetHashCode();
	}

	IEnumerator<TError> IEnumerable<TError>.GetEnumerator()
	{
		return GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
