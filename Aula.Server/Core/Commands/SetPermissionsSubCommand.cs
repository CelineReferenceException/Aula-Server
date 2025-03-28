﻿using Aula.Server.Common.Persistence;
using Aula.Server.Domain;
using Aula.Server.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Aula.Server.Core.Commands;

[CommandLineIgnore]
internal sealed class SetPermissionsSubCommand : Command
{
	private readonly ILogger<SetPermissionsSubCommand> _logger;

	private readonly CommandOption _permissionsOption = new()
	{
		Name = "p",
		Description = "The permission flags to set.",
		IsRequired = true,
		RequiresArgument = true,
		CanOverflow = false,
	};

	private readonly CommandOption _userIdOption = new()
	{
		Name = "u",
		Description = "The id of the user to set the permissions for.",
		IsRequired = true,
		RequiresArgument = true,
		CanOverflow = false,
	};

	public SetPermissionsSubCommand(
		ILogger<SetPermissionsSubCommand> logger,
		IServiceProvider serviceProvider)
		: base(serviceProvider)
	{
		_logger = logger;
		AddOptions(_userIdOption, _permissionsOption);
	}

	internal override String Name => "set";

	internal override String Description => "Overwrites the permissions of a user with the provided ones.";

	internal override async ValueTask Callback(IReadOnlyDictionary<String, String> args, CancellationToken cancellationToken)
	{
		var userIdArgument = args[_userIdOption.Name];

		if (!Snowflake.TryParse(userIdArgument, out var userId))
		{
			_logger.CommandFailed("The user id must be numeric.");
			return;
		}

		using var serviceScope = ServiceProvider.CreateScope();
		var dbContext = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

		var user = await dbContext.Users
			.AsTracking()
			.Where(u => u.Id == userId)
			.FirstOrDefaultAsync(cancellationToken);
		if (user is null)
		{
			_logger.CommandFailed("The user was not found.");
			return;
		}

		var permissionsArgument = args[_permissionsOption.Name];

		if (!Enum.TryParse(permissionsArgument, true, out Permissions permissions))
		{
			_logger.CommandFailed("Invalid permission flag value or format.");
			return;
		}

		user.Modify(permissions: permissions).ThrowIfFailed();
		user.UpdateConcurrencyStamp();

		try
		{
			_ = await dbContext.SaveChangesAsync(cancellationToken);
		}
		catch (DbUpdateConcurrencyException)
		{
			_logger.CommandFailed("Another task was working on the user while updating, try again.");
			return;
		}

		_logger.UserPermissionsUpdatedByCommand();
	}
}
