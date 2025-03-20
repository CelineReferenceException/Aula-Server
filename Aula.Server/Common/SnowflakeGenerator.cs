using Microsoft.Extensions.Options;

namespace Aula.Server.Common;

internal sealed class SnowflakeGenerator
{
	private static readonly DateTime s_epoch = new(2024, 12, 1, 12, 0, 0, DateTimeKind.Utc);
	private static readonly TimeSpan s_oneTickSpan = TimeSpan.FromTicks(1);
	private readonly Lock _newSnowflakeLock = new();
	private readonly UInt16 _workerId;
	private UInt16 _increment;
	private DateTime _lastOperationDate;

	public SnowflakeGenerator(IOptions<ApplicationOptions> applicationOptions)
	{
		var workerId = applicationOptions.Value.WorkerId.Value;
		ArgumentOutOfRangeException.ThrowIfLessThan<UInt16>(workerId, 0);
		ArgumentOutOfRangeException.ThrowIfGreaterThan<UInt16>(workerId, 1023);

		_workerId = workerId;
		_lastOperationDate = DateTime.UtcNow;
	}

	public async ValueTask<Snowflake> NewSnowflakeAsync()
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
		var snowflake = new Snowflake(s_epoch, _lastOperationDate, _workerId, _increment++);

		_newSnowflakeLock.Exit();
		return snowflake;
	}
}
