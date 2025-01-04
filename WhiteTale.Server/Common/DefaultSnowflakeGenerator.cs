using Microsoft.Extensions.Options;

namespace WhiteTale.Server.Common;

internal sealed class DefaultSnowflakeGenerator : ISnowflakeGenerator
{
	private static readonly DateTime s_epoch = new(2024, 12, 1, 12, 0, 0, DateTimeKind.Utc);
	private readonly Lock _newSnowflakeLock = new();
	private readonly UInt32 _workerId;
	private UInt32 _increment;
	private DateTime _lastOperationTime;

	public DefaultSnowflakeGenerator(IOptions<ApplicationOptions> applicationOptions)
	{
		var workerId = applicationOptions.Value.WorkerId;
		ArgumentOutOfRangeException.ThrowIfLessThan<UInt32>(workerId, 0);
		ArgumentOutOfRangeException.ThrowIfGreaterThan<UInt32>(workerId, 1023);

		_workerId = workerId;
		_lastOperationTime = DateTime.UtcNow;
	}

	public UInt64 NewSnowflake()
	{
		_newSnowflakeLock.Enter();

		if (++_increment >= 4096)
		{
			while (_lastOperationTime == DateTime.UtcNow)
			{
			}

			_increment = 1;
		}

		_lastOperationTime = DateTime.UtcNow;
		var sinceEpoch = _lastOperationTime - s_epoch;
		var snowflake = ((UInt64)sinceEpoch.TotalMilliseconds << 22) | (_workerId << 12) | _increment++;

		_newSnowflakeLock.Exit();
		return snowflake;
	}
}
