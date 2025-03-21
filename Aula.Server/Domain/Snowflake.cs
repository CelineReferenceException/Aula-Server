namespace Aula.Server.Domain;

internal readonly struct Snowflake
{
	private readonly UInt64 _value;

	internal Snowflake(UInt64 value)
	{
		ArgumentOutOfRangeException.ThrowIfZero(value);
		_value = value;
	}

	internal Snowflake(UInt64 millisecondsSinceEpoch, UInt16 workerId, UInt16 increment)
		: this((millisecondsSinceEpoch << 22) | (UInt32)(workerId << 12) | increment)
	{
	}

	internal Snowflake(DateTime epoch, DateTime instant, UInt16 workerId, UInt16 increment)
		: this((UInt64)(instant - epoch).TotalMilliseconds, workerId, increment)
	{
	}

	internal UInt64 Value => _value is not 0 ? _value : throw new InvalidOperationException("Value is not initialized.");

	internal UInt16 Increment => (UInt16)(Value & 0b1111_1111_1111);

	internal UInt16 WorkerId => (UInt16)((Value >> 12) & 0b11_1111_1111);

	internal UInt64 CreationDate => Value >> 22;

	public static implicit operator Snowflake(UInt64 value) => new(value);

	public static implicit operator UInt64(Snowflake value) => value.Value;
}
