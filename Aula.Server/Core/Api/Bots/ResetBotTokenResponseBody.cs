namespace Aula.Server.Core.Api.Bots;

internal sealed record ResetBotTokenResponseBody
{
	public required String Token { get; init; }
}
