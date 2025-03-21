namespace Aula.Server.Core.Features.Messages;

internal static class MessageRateLimitingPolicies
{
	private const String Prefix = nameof(MessageRateLimitingPolicies);
	internal const String SendMessage = $"{Prefix}.{nameof(SendMessage)}";
	internal const String RemoveMessage = $"{Prefix}.{nameof(RemoveMessage)}";
}
