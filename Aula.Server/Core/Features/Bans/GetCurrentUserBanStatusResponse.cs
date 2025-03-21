namespace Aula.Server.Core.Features.Bans;

internal sealed record GetCurrentUserBanStatusResponse
{
	public Boolean Banned { get; init; }
}
