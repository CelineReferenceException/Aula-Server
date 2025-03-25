namespace Aula.Server.Core.Api.Bans;

internal sealed record GetCurrentUserBanStatusResponse
{
	public Boolean Banned { get; init; }
}
