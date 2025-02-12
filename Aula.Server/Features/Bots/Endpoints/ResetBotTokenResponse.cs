namespace Aula.Server.Features.Bots.Endpoints;

internal sealed record ResetBotTokenResponse
{
	public required String Token { get; init; }
}
