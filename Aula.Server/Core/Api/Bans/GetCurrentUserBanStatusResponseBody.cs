namespace Aula.Server.Core.Api.Bans;

internal sealed record GetCurrentUserBanStatusResponseBody
{
	public Boolean Banned { get; init; }
}
