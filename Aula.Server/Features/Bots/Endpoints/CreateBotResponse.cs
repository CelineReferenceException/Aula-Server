namespace Aula.Server.Features.Bots.Endpoints;

internal sealed record CreateBotResponse
{
	public required UserData User { get; set; }

	public required String Token { get; init; }
}
