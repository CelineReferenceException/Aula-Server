namespace WhiteTale.Server.Features.Bans.Endpoints;

internal sealed record ImBannedResponse
{
	public Boolean Banned { get; init; }
}
