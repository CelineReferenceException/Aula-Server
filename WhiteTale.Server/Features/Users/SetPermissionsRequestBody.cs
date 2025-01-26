namespace WhiteTale.Server.Features.Users;

internal sealed class SetPermissionsRequestBody
{
	public required Permissions Permissions { get; set; }
}
