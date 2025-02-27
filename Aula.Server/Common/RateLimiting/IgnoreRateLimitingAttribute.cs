namespace Aula.Server.Common.RateLimiting;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
internal sealed class IgnoreRateLimitingAttribute : Attribute
{
}
