namespace Aula.Server.Common;

internal readonly struct Snowflake
{
	internal Snowflake(UInt64 value)
	{
		Value = value;
	}

	internal UInt64 Value { get; }

	internal UInt16 Increment => (UInt16)(Value & 0b1111_1111_1111);

	internal UInt16 WorkerId => (UInt16)((Value >> 12) & 0b11_1111_1111);

	internal UInt64 CreationDate => Value >> 22;

	public static implicit operator Snowflake(UInt64 value) => new(value);

	public static implicit operator UInt64(Snowflake value) => value.Value;
}
