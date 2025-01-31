namespace WhiteTale.Server.Features.Users.Commands;

internal static partial class LoggerExtensions
{
	[LoggerMessage(LogLevel.Information, "Permissions updated.")]
	internal static partial void UserPermissionsUpdatedByCommand(this ILogger logger);

	[LoggerMessage(LogLevel.Information, "Permissions: {permissions}")]
	internal static partial void ExistingPermissions(this ILogger logger, String permissions);
}
