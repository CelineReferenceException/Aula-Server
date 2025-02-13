namespace Aula.Server.Features.Bans.Endpoints;

internal sealed record GetCurrentUserBanResponse
{
	public Boolean Banned { get; init; }
}
