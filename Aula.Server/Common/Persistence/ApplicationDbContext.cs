using System.Diagnostics.CodeAnalysis;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Aula.Server.Common.Persistence;

internal sealed class ApplicationDbContext : DbContext
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

	internal DbSet<User> Users => Set<User>();

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

		_ = userModel.Property(x => x.UserName)
			.IsRequired()
			.HasMaxLength(User.UserNameMaximumLength);

		_ = userModel.Property(x => x.Email)
			.IsRequired(false);

		_ = userModel.Property(x => x.EmailConfirmed)
			.IsRequired();

		_ = userModel.Property(x => x.PasswordHash)
			.IsRequired(false)
			.HasMaxLength(User.PasswordMaximumLength);

		_ = userModel.Property(x => x.SecurityStamp)
			.IsRequired(false);

		_ = userModel.Property(x => x.AccessFailedCount)
			.IsRequired();

		_ = userModel.Property(x => x.LockoutEndTime)
			.IsRequired(false);

		_ = userModel.Property(x => x.Permissions)
			.IsRequired();

		_ = userModel.Property(x => x.DisplayName)
			.IsRequired()
			.HasMaxLength(User.DisplayNameMaximumLength);

		_ = userModel.Property(x => x.Description)
			.IsRequired()
			.HasMaxLength(User.DescriptionMaximumLength);

		_ = userModel.Property(x => x.Type)
			.IsRequired();

		_ = userModel.Property(x => x.Presence)
			.IsRequired();

		_ = userModel.Property(x => x.CreationDate)
			.IsRequired();

		_ = userModel.Property(x => x.CurrentRoomId)
			.IsRequired(false);

		_ = userModel.Property(x => x.IsRemoved)
			.IsRequired();

		_ = userModel.Property(x => x.ConcurrencyStamp)
			.IsRequired()
			.IsConcurrencyToken()
			.HasMaxLength(guidMaximumCharacterLength);

		_ = userModel
			.HasIndex(x => new
			{
				x.UserName,
			})
			.HasDatabaseName($"IX_{nameof(User)}_{nameof(User.UserName)}");

		_ = userModel
			.HasIndex(x => new
			{
				x.Email,
			})
			.HasDatabaseName($"IX_{nameof(User)}_{nameof(User.Email)}");

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

		_ = roomModel.Property(x => x.CreationDate)
			.IsRequired();

		_ = roomModel.HasMany(x => x.Connections)
			.WithOne(x => x.SourceRoom)
			.HasForeignKey(x => x.SourceRoomId)
			.HasPrincipalKey(x => x.Id);

		_ = roomModel.Navigation(x => x.Connections)
			.AutoInclude();

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

		_ = messageModel.Property(x => x.RoomId)
			.IsRequired();

		_ = messageModel
			.HasOne(x => x.JoinData)
			.WithOne(x => x.Message)
			.HasForeignKey<MessageUserJoin>();

		_ = messageModel.Navigation(x => x.JoinData)
			.AutoInclude();

		_ = messageModel
			.HasOne(x => x.LeaveData)
			.WithOne(x => x.Message)
			.HasForeignKey<MessageUserLeave>();

		_ = messageModel.Navigation(x => x.LeaveData)
			.AutoInclude();

		_ = messageModel.Property(x => x.Content)
			.IsRequired(false)
			.HasMaxLength(Message.ContentMaximumLength);

		_ = messageModel.Property(x => x.CreationDate)
			.IsRequired();

		_ = messageModel.Property(x => x.IsRemoved)
			.IsRequired();

		var messageUserJoinModel = modelBuilder.Entity<MessageUserJoin>();

		_ = messageUserJoinModel.ToTable($"{nameof(Messages)}_{nameof(MessageUserJoin)}");

		_ = messageUserJoinModel.Property(x => x.MessageId)
			.IsRequired()
			.ValueGeneratedNever();
		_ = messageUserJoinModel.HasKey(x => x.MessageId);

		_ = messageUserJoinModel.Property(x => x.UserId)
			.IsRequired();

		var messageUserLeaveModel = modelBuilder.Entity<MessageUserLeave>();

		_ = messageUserLeaveModel.ToTable($"{nameof(Messages)}_{nameof(MessageUserLeave)}");

		_ = messageUserLeaveModel.Property(x => x.MessageId)
			.IsRequired()
			.ValueGeneratedNever();
		_ = messageUserLeaveModel.HasKey(x => x.MessageId);

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

		_ = banModel.Property(x => x.CreationDate)
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

		await PublishDomainEventsAsync(cancellationToken);

		ChangeTracker.Clear();

		return entriesWritten;
	}

	internal async Task<Int32> SaveChangesWithConcurrencyCheckBypassAsync(CancellationToken cancellationToken = default)
	{
		var saved = false;
		var entriesWritten = 0;

		while (!saved)
		{
			try
			{
				entriesWritten = await base.SaveChangesAsync(cancellationToken);
				saved = true;
			}
			catch (DbUpdateConcurrencyException ex)
			{
				foreach (var entry in ex.Entries)
				{
					var databaseValues = await entry.GetDatabaseValuesAsync(cancellationToken);
					if (databaseValues is null)
					{
						throw;
					}

					// Refresh original values to bypass next concurrency check
					entry.OriginalValues.SetValues(databaseValues);
				}
			}
		}

		await PublishDomainEventsAsync(cancellationToken);

		ChangeTracker.Clear();

		return entriesWritten;
	}

	private async ValueTask PublishDomainEventsAsync(CancellationToken cancellationToken = default)
	{
		foreach (var entry in ChangeTracker.Entries<IDomainEntity>())
		{
			var entity = entry.Entity;

			foreach (var domainEvent in entity.Events)
			{
				await _publisher.Publish(domainEvent, cancellationToken);
			}

			entity.ClearEvents();
		}
	}
}
