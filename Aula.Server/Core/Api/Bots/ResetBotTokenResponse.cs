namespace Aula.Server.Core.Api.Bots;

internal sealed record ResetBotTokenResponse
{
	public required String Token { get; init; }
}
