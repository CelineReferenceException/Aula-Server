namespace Aula.Server.Core.RateLimiting;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
internal sealed class IgnoreRateLimitingAttribute : Attribute
{
}
