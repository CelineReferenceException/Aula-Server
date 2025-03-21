namespace Aula.Server.Core.Features.Bans.Endpoints;

internal sealed record GetCurrentUserBanStatusResponse
{
	public Boolean Banned { get; init; }
}
