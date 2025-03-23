namespace Aula.Server.Common.Results;

internal readonly ref struct ResultProblemValues : IReadOnlyList<ResultProblem>, IEquatable<ResultProblemValues>
{
	private readonly Items<ResultProblem> _values;

	internal ResultProblemValues(Items<ResultProblem> values)
	{
		_values = values;
	}

	public Int32 Count => _values.Count;

	internal static ResultProblemValues Empty => new();

	public ResultProblem this[Int32 index]
	{
		get => _values[index];
		set => throw new NotSupportedException();
	}

	public static Boolean operator ==(ResultProblemValues left, ResultProblemValues right) => left.Equals(right);

	public static Boolean operator !=(ResultProblemValues left, ResultProblemValues right) => !left.Equals(right);

	public Items<ResultProblem>.Enumerator GetEnumerator()
	{
		return _values.GetEnumerator();
	}

	public Boolean Equals(ResultProblemValues other)
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

	IEnumerator<ResultProblem> IEnumerable<ResultProblem>.GetEnumerator()
	{
		return GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
