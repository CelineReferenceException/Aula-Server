namespace WhiteTale.Server.Domain.Users;

internal sealed class User : IdentityUser<UInt64>
{
	public User(String userName) : base(userName)
	{
	}

	public new required UInt64 Id
	{
		get => base.Id;
		init => base.Id = value;
	}

	public Permissions Permissions { get; set; }
}
