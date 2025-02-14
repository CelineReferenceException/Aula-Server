namespace Aula.Server.Features.Bans.Endpoints;

internal sealed record GetCurrentUserBanStatusResponse
{
	public Boolean Banned { get; init; }
}
