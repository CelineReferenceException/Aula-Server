﻿using System.Text;

namespace Aula.Server.Core.Features.Users.Commands;

internal sealed class ListPermissionsCommand : SubCommand
{
	private readonly ILogger<PermissionsCommand> _logger;

	public ListPermissionsCommand(ILogger<PermissionsCommand> logger, IServiceProvider serviceProvider) : base(serviceProvider)
	{
		_logger = logger;
	}

	internal override String Name => "list";

	internal override String Description => "Shows a list of all the existing permissions and their corresponding flags.";

	internal override ValueTask Callback(IReadOnlyDictionary<String, String> args, CancellationToken cancellationToken)
	{
		var permissionsMessage = new StringBuilder(Environment.NewLine);

		foreach (var permission in Enum.GetValues<Permissions>())
		{
			_ = permissionsMessage.AppendLine($"- {permission}: {(Int32)permission}");
		}

		_logger.ExistingPermissions(permissionsMessage.ToString());
		return ValueTask.CompletedTask;
	}
}
