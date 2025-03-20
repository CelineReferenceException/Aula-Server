using System.Collections;
using Aula.Server.Common.Collections;

namespace Aula.Server.Common.Validation;

internal readonly struct CustomValidationProblemValues : IReadOnlyList<CustomValidationProblem>, IEquatable<CustomValidationProblemValues>
{
	internal static CustomValidationProblemValues Empty => new();

	private readonly Items<CustomValidationProblem> _values;

	internal CustomValidationProblemValues(Items<CustomValidationProblem> values)
	{
		_values = values;
	}

	internal Int32 Count => _values.Count;

	internal CustomValidationProblem this[Int32 index]
	{
		get => _values[index];
		set => throw new NotSupportedException();
	}

	public Items<CustomValidationProblem>.Enumerator GetEnumerator()
	{
		return _values.GetEnumerator();
	}

	public Boolean Equals(CustomValidationProblemValues other)
	{
		return _values.Equals(other._values);
	}

	public override Boolean Equals(Object? obj)
	{
		return obj is CustomValidationProblemValues other && Equals(other);
	}

	public override Int32 GetHashCode()
	{
		return _values.GetHashCode();
	}

	public static Boolean operator ==(CustomValidationProblemValues left, CustomValidationProblemValues right) => left.Equals(right);

	public static Boolean operator !=(CustomValidationProblemValues left, CustomValidationProblemValues right) => !left.Equals(right);

	CustomValidationProblem IReadOnlyList<CustomValidationProblem>.this[Int32 index] => this[index];

	Int32 IReadOnlyCollection<CustomValidationProblem>.Count => Count;

	IEnumerator<CustomValidationProblem> IEnumerable<CustomValidationProblem>.GetEnumerator()
	{
		return GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
