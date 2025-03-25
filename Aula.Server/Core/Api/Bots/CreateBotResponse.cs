namespace Aula.Server.Core.Api.Bots;

internal sealed record CreateBotResponse
{
	public required UserData User { get; set; }

	public required String Token { get; init; }
}
