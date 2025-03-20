namespace Aula.Server.Common.Validation;

internal readonly struct CustomValidationResult : IEquatable<CustomValidationResult>
{
	internal static CustomValidationResult Success => new();

	internal CustomValidationProblemValues Problems { get; }

	internal Boolean Succeeded => Problems.Count is 0;

	internal CustomValidationResult(CustomValidationProblemValues problems)
	{
		Problems = problems;
	}

	public Boolean Equals(CustomValidationResult other)
	{
		return Problems.Equals(other.Problems);
	}

	public override Boolean Equals(Object? obj)
	{
		return obj is CustomValidationResult other && Equals(other);
	}

	public override Int32 GetHashCode()
	{
		return Problems.GetHashCode();
	}

	public static Boolean operator ==(CustomValidationResult left, CustomValidationResult right) => left.Equals(right);

	public static Boolean operator !=(CustomValidationResult left, CustomValidationResult right) => !left.Equals(right);
}
