namespace Aula.Server.Common.Results;

internal readonly struct ResultProblemValues : IReadOnlyList<ResultProblem>, IEquatable<ResultProblemValues>
{
	internal static ResultProblemValues Empty => new();

	private readonly Items<ResultProblem> _values;

	internal ResultProblemValues(Items<ResultProblem> values)
	{
		_values = values;
	}

	internal Int32 Count => _values.Count;

	internal ResultProblem this[Int32 index]
	{
		get => _values[index];
		set => throw new NotSupportedException();
	}

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
		return obj is ResultProblemValues other && Equals(other);
	}

	public override Int32 GetHashCode()
	{
		return _values.GetHashCode();
	}

	public static Boolean operator ==(ResultProblemValues left, ResultProblemValues right) => left.Equals(right);

	public static Boolean operator !=(ResultProblemValues left, ResultProblemValues right) => !left.Equals(right);

	ResultProblem IReadOnlyList<ResultProblem>.this[Int32 index] => this[index];

	Int32 IReadOnlyCollection<ResultProblem>.Count => Count;

	IEnumerator<ResultProblem> IEnumerable<ResultProblem>.GetEnumerator()
	{
		return GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
