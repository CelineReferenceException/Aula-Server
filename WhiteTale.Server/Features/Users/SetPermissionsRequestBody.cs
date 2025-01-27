namespace WhiteTale.Server.Features.Users;

internal sealed record SetPermissionsRequestBody
{
	public required Permissions Permissions { get; set; }
}
