namespace Aula.Server.Core.Features.Bots.Endpoints;

internal sealed record ResetBotTokenResponse
{
	public required String Token { get; init; }
}
