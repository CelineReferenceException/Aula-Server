namespace Aula.Server.Core.Features.Bots;

internal sealed record ResetBotTokenResponse
{
	public required String Token { get; init; }
}
