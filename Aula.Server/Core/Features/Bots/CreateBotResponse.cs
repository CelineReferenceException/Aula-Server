namespace Aula.Server.Core.Features.Bots;

internal sealed record CreateBotResponse
{
	public required UserData User { get; set; }

	public required String Token { get; init; }
}
