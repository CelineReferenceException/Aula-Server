﻿using Aula.Server.Domain.Users;

namespace Aula.Server.Core.Api.Identity;

/// <summary>
///     User identity related configurations.
/// </summary>
internal sealed class IdentityFeatureOptions
{
	internal const String SectionName = "Identity";

	/// <summary>
	///     The Uri to redirect to after users confirm their emails, should be a trusted origin.
	/// </summary>
	public Uri? ConfirmEmailRedirectUri { get; set; }

	/// <summary>
	///     The Uri where users should reset their password, should be a trusted origin.
	/// </summary>
	public Uri? ResetPasswordRedirectUri { get; set; }

	/// <summary>
	///     The default permissions to assign to users after registration.
	/// </summary>
	public Permissions DefaultPermissions { get; set; }
}
