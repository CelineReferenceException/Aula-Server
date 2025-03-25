using System.Linq.Expressions;
using Aula.Server.Domain;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Aula.Server.Common.Persistence;

internal sealed class SnowflakeToUInt64Converter : ValueConverter<Snowflake, UInt64>
{
	internal SnowflakeToUInt64Converter()
		: base(ToProvider(), FromProvider())
	{
	}

	private static Expression<Func<Snowflake, UInt64>> ToProvider()
	{
		return static s => s;
	}

	private static Expression<Func<UInt64, Snowflake>> FromProvider()
	{
		return static n => n;
	}
}
