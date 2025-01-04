namespace WhiteTale.Server.Common;

internal interface ISnowflakeGenerator
{
	ValueTask<UInt64> NewSnowflakeAsync();
}
