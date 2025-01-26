namespace WhiteTale.Server.Features.Users;

internal static partial class LoggerExtensions
{
	[LoggerMessage(LogLevel.Information, "Permissions updated.")]
	internal static partial void UserPermissionsUpdatedByCommand(this ILogger logger);

	[LoggerMessage(LogLevel.Information, "Permissions:\n{permissions}")]
	internal static partial void ExistingPermissions(this ILogger logger, String permissions);
}
