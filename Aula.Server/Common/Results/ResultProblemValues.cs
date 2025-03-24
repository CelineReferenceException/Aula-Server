namespace Aula.Server.Common.Results;

internal readonly ref struct ResultProblemValues<TProblem> : IReadOnlyList<TProblem>, IEquatable<ResultProblemValues<TProblem>>
	where TProblem : class?
{
	private readonly Items<TProblem> _values;

	internal ResultProblemValues(Items<TProblem> values)
	{
		_values = values;
	}

	public Int32 Count => _values.Count;

	internal static ResultProblemValues<TProblem> Empty => new();

	public TProblem this[Int32 index]
	{
		get => _values[index];
		set => throw new NotSupportedException();
	}

	public static Boolean operator ==(ResultProblemValues<TProblem> left, ResultProblemValues<TProblem> right) => left.Equals(right);

	public static Boolean operator !=(ResultProblemValues<TProblem> left, ResultProblemValues<TProblem> right) => !left.Equals(right);

	public Items<TProblem>.Enumerator GetEnumerator()
	{
		return _values.GetEnumerator();
	}

	public Boolean Equals(ResultProblemValues<TProblem> other)
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

	IEnumerator<TProblem> IEnumerable<TProblem>.GetEnumerator()
	{
		return GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
