using System.Diagnostics.CodeAnalysis;
using MediatR;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace WhiteTale.Server.Common.Persistence;

internal sealed class ApplicationDbContext : IdentityUserContext<User, UInt64>
{
	private readonly IHostEnvironment _hostEnvironment;
	private readonly IPublisher _publisher;

	public ApplicationDbContext(
		DbContextOptions<ApplicationDbContext> options,
		IHostEnvironment hostEnvironment,
		IPublisher publisher) : base(options)
	{
		_hostEnvironment = hostEnvironment;
		_publisher = publisher;
	}

	internal DbSet<Ban> Bans => Set<Ban>();

	internal DbSet<Room> Rooms => Set<Room>();

	internal DbSet<RoomConnection> RoomConnections => Set<RoomConnection>();

	internal DbSet<Message> Messages => Set<Message>();

	[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		if (_hostEnvironment.IsDevelopment())
		{
			var loggerFactory = LoggerFactory.Create(builder => builder.AddLogging());
			_ = optionsBuilder.UseLoggerFactory(loggerFactory);
		}

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

		_ = userModel.Property(x => x.Permissions)
			.IsRequired();

		_ = userModel.Property(x => x.DisplayName)
			.IsRequired()
			.HasMaxLength(User.DisplayNameMaximumLength);

		_ = userModel.Property(x => x.Description)
			.HasMaxLength(User.DescriptionMaximumLength);

		_ = userModel.Property(x => x.OwnerType)
			.IsRequired();

		_ = userModel.Property(x => x.Presence)
			.IsRequired();

		_ = userModel.Property(x => x.CreationTime)
			.IsRequired();

		_ = userModel.Property(x => x.CurrentRoomId)
			.IsRequired(false);

		_ = userModel.Property(x => x.ConcurrencyStamp)
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
			.HasMaxLength(Room.DescriptionMaximumLength);

		_ = roomModel.Property(x => x.IsEntrance)
			.IsRequired();

		_ = roomModel.Property(x => x.CreationTime)
			.IsRequired();

		_ = roomModel.Property(x => x.ConcurrencyStamp)
			.IsRequired()
			.IsConcurrencyToken()
			.HasMaxLength(guidMaximumCharacterLength);

		_ = roomModel.Property(x => x.IsRemoved)
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
			.ValueGeneratedNever()
			.HasConversion<Int64>();
		_ = messageModel.HasKey(x => x.Id);

		_ = messageModel.Property(x => x.Type)
			.IsRequired();

		_ = messageModel.Property(x => x.Flags)
			.IsRequired();

		_ = messageModel.Property(x => x.AuthorType)
			.IsRequired();

		_ = messageModel.Property(x => x.AuthorId)
			.IsRequired(false);

		_ = messageModel.Property(x => x.TargetType)
			.IsRequired();

		_ = messageModel.Property(x => x.TargetId)
			.IsRequired();

		_ = messageModel
			.HasOne(x => x.JoinData)
			.WithOne(x => x.Message)
			.HasForeignKey<MessageUserJoin>();

		_ = messageModel
			.HasOne(x => x.LeaveData)
			.WithOne(x => x.Message)
			.HasForeignKey<MessageUserLeave>();

		_ = messageModel.Property(x => x.Content)
			.IsRequired(false)
			.HasMaxLength(Message.ContentMaximumLength);

		_ = messageModel.Property(x => x.CreationTime)
			.IsRequired();

		_ = messageModel.Property(x => x.IsRemoved)
			.IsRequired();

		var messageUserJoinModel = modelBuilder.Entity<MessageUserJoin>();

		_ = messageUserJoinModel.ToTable($"{nameof(Messages)}_{nameof(MessageUserJoin)}");

		_ = messageUserJoinModel.Property(x => x.Id)
			.IsRequired()
			.ValueGeneratedNever();
		_ = messageUserJoinModel.HasKey(x => x.Id);

		_ = messageUserJoinModel.Property(x => x.UserId)
			.IsRequired();

		var messageUserLeaveModel = modelBuilder.Entity<MessageUserLeave>();

		_ = messageUserLeaveModel.ToTable($"{nameof(Messages)}_{nameof(MessageUserLeave)}");

		_ = messageUserLeaveModel.Property(x => x.Id)
			.IsRequired()
			.ValueGeneratedNever();
		_ = messageUserLeaveModel.HasKey(x => x.Id);

		_ = messageUserLeaveModel.Property(x => x.UserId)
			.IsRequired();

		_ = messageUserLeaveModel.Property(x => x.RoomId)
			.IsRequired();

		var banModel = modelBuilder.Entity<Ban>();

		_ = banModel.Property(x => x.Id)
			.IsRequired()
			.ValueGeneratedNever();
		_ = banModel.HasKey(x => x.Id);

		_ = banModel.Property(x => x.Type)
			.IsRequired();

		_ = banModel.Property(x => x.ExecutorId)
			.IsRequired(false);

		_ = banModel.Property(x => x.Reason)
			.IsRequired(false)
			.HasMaxLength(Ban.ReasonMaximumLength);

		_ = banModel.Property(x => x.TargetId)
			.IsRequired(false);

		_ = banModel.Property(x => x.CreationTime)
			.IsRequired();

		_ = banModel
			.HasIndex(x => new
			{
				x.TargetId,
			})
			.HasDatabaseName($"IX_{nameof(Ban)}_{nameof(Ban.TargetId)}");

		base.OnModelCreating(modelBuilder);
	}

	public override async Task<Int32> SaveChangesAsync(CancellationToken cancellationToken = default)
	{
		var entriesWritten = await base.SaveChangesAsync(cancellationToken);

		var domainEvents = ChangeTracker
			.Entries<IDomainEntity>()
			.SelectMany(x => x.Entity.Events);

		foreach (var domainEvent in domainEvents)
		{
			await _publisher.Publish(domainEvent, cancellationToken);
		}

		return entriesWritten;
	}
}
