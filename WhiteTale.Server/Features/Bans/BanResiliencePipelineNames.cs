namespace WhiteTale.Server.Features.Bans;

internal static class BanResiliencePipelineNames
{
	private const String Prefix = nameof(BanResiliencePipelineNames);

	internal const String CleanUser = $"{Prefix}.{nameof(CleanUser)}";
}
