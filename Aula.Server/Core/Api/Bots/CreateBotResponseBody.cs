namespace Aula.Server.Core.Api.Bots;

internal sealed record CreateBotResponseBody
{
	public required UserData User { get; set; }

	public required String Token { get; init; }
}
