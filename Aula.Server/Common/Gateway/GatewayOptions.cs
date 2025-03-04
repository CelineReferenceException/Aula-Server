using System.ComponentModel.DataAnnotations;

namespace Aula.Server.Common.Gateway;

internal sealed class GatewayOptions
{
	internal const String SectionName = "Gateway";

	[Length(1, Int32.MaxValue)]
	public Int32 SecondsToExpire { get; set; } = 60 * 5;
}
