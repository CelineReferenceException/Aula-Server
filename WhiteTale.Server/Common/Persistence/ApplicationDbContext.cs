using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WhiteTale.Server.Domain.Characters;
using WhiteTale.Server.Domain.Messages;
using WhiteTale.Server.Domain.Rooms;
using WhiteTale.Server.Domain.Users;

namespace WhiteTale.Server.Common.Persistence;

internal sealed class ApplicationDbContext : IdentityUserContext<User, UInt64>
{
	private readonly IConfiguration _configuration;
	private readonly IOptions<PersistenceOptions> _persistenceOptions;

	public ApplicationDbContext(IConfiguration configuration, IOptions<PersistenceOptions> persistenceOptions)
	{
		_configuration = configuration;
		_persistenceOptions = persistenceOptions;
	}

	internal DbSet<Character> Characters => Set<Character>();

	internal DbSet<Room> Rooms => Set<Room>();

	internal DbSet<RoomConnection> RoomConnections => Set<RoomConnection>();

	internal DbSet<Message> Messages => Set<Message>();

	[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		if (!_persistenceOptions.Value.UseInMemoryDatabase)
		{
			_ = optionsBuilder.UseSqlite(_configuration.GetConnectionString("Default"));
		}
		else
		{
			_ = optionsBuilder.UseInMemoryDatabase("InMemoryDatabase");
		}

		var loggerFactory = LoggerFactory.Create(builder => builder.AddLogging());
		_ = optionsBuilder.UseLoggerFactory(loggerFactory);

		base.OnConfiguring(optionsBuilder);
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		const Int32 guidMaximumCharacterLength = 38;

		var userModel = modelBuilder.Entity<User>();

		_ = userModel.Property(x => x.Id)
			.IsRequired()
			.ValueGeneratedNever();
		_ = userModel.HasKey(x => x.Id);

		var characterModel = modelBuilder.Entity<Character>();

		_ = characterModel.Property(x => x.Id)
			.IsRequired()
			.ValueGeneratedNever();
		_ = characterModel.HasKey(x => x.Id);

		_ = characterModel.Property(x => x.DisplayName)
			.IsRequired()
			.HasMaxLength(Character.DisplayNameMaximumLength);

		_ = characterModel.Property(x => x.Description)
			.HasMaxLength(Character.DescriptionMaximumLength);

		_ = characterModel.Property(x => x.OwnerType)
			.IsRequired();

		_ = characterModel.Property(x => x.Permissions)
			.IsRequired();

		_ = characterModel.Property(x => x.Presence)
			.IsRequired();

		_ = characterModel.Property(x => x.CreationTime)
			.IsRequired();

		_ = characterModel.Property(x => x.CurrentRoomId)
			.IsRequired(false);

		_ = characterModel.Property(x => x.ConcurrencyStamp)
			.IsRequired()
			.IsConcurrencyToken()
			.HasMaxLength(guidMaximumCharacterLength);

		var roomModel = modelBuilder.Entity<Room>();

		_ = roomModel.Property(x => x.Id)
			.IsRequired()
			.ValueGeneratedNever();
		_ = roomModel.HasKey(x => x.Id);

		_ = roomModel.Property(x => x.Name)
			.IsRequired()
			.HasMaxLength(Room.NameMaximumLength);

		_ = roomModel.Property(x => x.Description)
			.IsRequired()
			.HasMaxLength(Room.DescriptionMaximumLength);

		_ = roomModel.Property(x => x.IsEntrance)
			.IsRequired();

		var roomConnectionModel = modelBuilder.Entity<RoomConnection>();

		_ = roomConnectionModel.Property(x => x.Id)
			.IsRequired()
			.ValueGeneratedNever();
		_ = roomConnectionModel.HasKey(x => x.Id);

		_ = roomConnectionModel.Property(x => x.SourceRoomId)
			.IsRequired();

		_ = roomConnectionModel.Property(x => x.TargetRoomId)
			.IsRequired();

		var messageModel = modelBuilder.Entity<Message>();

		_ = messageModel.Property(x => x.Id)
			.IsRequired()
			.ValueGeneratedNever();
		_ = messageModel.HasKey(x => x.Id);

		_ = messageModel.Property(x => x.Flags)
			.IsRequired();

		_ = messageModel.Property(x => x.AuthorId)
			.IsRequired();

		_ = messageModel.Property(x => x.Target)
			.IsRequired();

		_ = messageModel.Property(x => x.RoomId)
			.IsRequired(false);

		_ = messageModel.Property(x => x.Content)
			.IsRequired()
			.HasMaxLength(Message.ContentMaximumLength);

		_ = messageModel.Property(x => x.CreationTime)
			.IsRequired();

		base.OnModelCreating(modelBuilder);
	}
}
