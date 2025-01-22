namespace WhiteTale.Server.Features.Gateway;

internal enum Intents
{
	Users = 1 << 0,
	Rooms = 1 << 1,
	Messages = 1 << 2,
}
