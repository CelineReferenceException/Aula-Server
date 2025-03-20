using Microsoft.Extensions.Options;

namespace Aula.Server.Common;

internal sealed class SnowflakeGenerator
{
	private static readonly DateTime s_epoch = new(2024, 12, 1, 12, 0, 0, DateTimeKind.Utc);
	private static readonly TimeSpan s_oneTickSpan = TimeSpan.FromTicks(1);
	private readonly Lock _newSnowflakeLock = new();
	private readonly UInt32 _workerId;
	private UInt32 _increment;
	private DateTime _lastOperationDate;

	public SnowflakeGenerator(IOptions<ApplicationOptions> applicationOptions)
	{
		var workerId = applicationOptions.Value.WorkerId.Value;
		ArgumentOutOfRangeException.ThrowIfLessThan<UInt32>(workerId, 0);
		ArgumentOutOfRangeException.ThrowIfGreaterThan<UInt32>(workerId, 1023);

		_workerId = workerId;
		_lastOperationDate = DateTime.UtcNow;
	}

	public async ValueTask<UInt64> NewSnowflakeAsync()
	{
		_newSnowflakeLock.Enter();

		if (++_increment >= 4096)
		{
			while (_lastOperationDate == DateTime.UtcNow)
			{
				await Task.Delay(s_oneTickSpan);
			}

			_increment = 0;
		}

		_lastOperationDate = DateTime.UtcNow;
		var sinceEpoch = _lastOperationDate - s_epoch;
		var snowflake = ((UInt64)sinceEpoch.TotalMilliseconds << 22) | (_workerId << 12) | _increment++;

		_newSnowflakeLock.Exit();
		return snowflake;
	}
}
