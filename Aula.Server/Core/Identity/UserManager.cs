using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using Aula.Server.Core.Domain;
using Aula.Server.Core.Domain.Users;
using Aula.Server.Core.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Aula.Server.Core.Identity;

internal sealed class UserManager
{
	internal const String UserNameAllowedCharacters = $"{LowercaseCharacters}{Digits}._";
	private const String UppercaseCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
	private const String LowercaseCharacters = "abcdefghijklmnopqrstuvwxyz";
	private const String Digits = "0123456789";

	private static readonly ConcurrentDictionary<Snowflake, PendingEmailConfirmation> s_pendingEmailConfirmations = new();
	private static readonly ConcurrentDictionary<Snowflake, PendingPasswordReset> s_pendingPasswordResets = new();
	private static readonly TimeSpan s_pendingEmailConfirmationsLifeTime = TimeSpan.FromMinutes(15);
	private static readonly TimeSpan s_pendingPasswordResetsLifeTime = TimeSpan.FromMinutes(15);
	private readonly ApplicationDbContext _dbContext;
	private readonly PasswordHasher<User> _passwordHasher;
	private readonly List<User> _users = [];

	public UserManager(
		ApplicationDbContext dbContext,
		PasswordHasher<User> passwordHasher,
		IOptions<IdentityOptions> identityOptions)
	{
		_dbContext = dbContext;
		_passwordHasher = passwordHasher;
		Options = identityOptions.Value;
	}

	internal IdentityOptions Options { get; }

	internal static void CleanPendingEmailConfirmations()
	{
		var now = DateTime.UtcNow;

		foreach (var pendingEmailConfirmation in s_pendingEmailConfirmations)
		{
			if (now - pendingEmailConfirmation.Value.CreationDate > s_pendingEmailConfirmationsLifeTime)
			{
				_ = s_pendingEmailConfirmations.TryRemove(pendingEmailConfirmation.Key, out _);
			}
		}
	}

	internal static void CleanPendingPasswordResets()
	{
		var now = DateTime.UtcNow;

		foreach (var pendingPasswordReset in s_pendingPasswordResets)
		{
			if (now - pendingPasswordReset.Value.CreationDate > s_pendingPasswordResetsLifeTime)
			{
				_ = s_pendingEmailConfirmations.TryRemove(pendingPasswordReset.Key, out _);
			}
		}
	}

	[SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Prefer to use an instance")]
	internal Snowflake? GetUserId(ClaimsPrincipal user)
	{
		var idClaimValue = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
		if (idClaimValue is null ||
		    !Snowflake.TryParse(idClaimValue, out var id))
		{
			return null;
		}

		return id;
	}

	internal async Task<User?> GetUserAsync(ClaimsPrincipal user)
	{
		var id = GetUserId(user);
		if (id is null)
		{
			return null;
		}

		return await FindByIdAsync((Snowflake)id);
	}

	internal async ValueTask<User?> FindByIdAsync(Snowflake userId)
	{
		var user = _users.FirstOrDefault(u => u.Id == userId);
		if (user is not null)
		{
			return user;
		}

		user = await _dbContext.Users
			.Where(u => u.Id == userId)
			.FirstOrDefaultAsync();

		if (user is not null)
		{
			_users.Add(user);
		}

		return user;
	}

	internal async ValueTask<User?> FindByEmailAsync(String email)
	{
		var user = _users.FirstOrDefault(u => u.Email == email);
		if (user is not null)
		{
			return user;
		}

		user = await _dbContext.Users
			.Where(u => u.Email == email)
			.FirstOrDefaultAsync();

		if (user is not null)
		{
			_users.Add(user);
		}

		return user;
	}

	internal async ValueTask<User?> FindByUserNameAsync(String userName)
	{
		var user = _users.FirstOrDefault(u => u.UserName == userName);
		if (user is not null)
		{
			return user;
		}

		user = await _dbContext.Users
			.Where(u => u.UserName == userName)
			.FirstOrDefaultAsync();

		if (user is not null)
		{
			_users.Add(user);
		}

		return user;
	}

	internal async Task<RegisterUserResult> RegisterAsync(User user)
	{
		if (await _dbContext.Users.AnyAsync(u => u.Email == user.Email))
		{
			return RegisterUserResult.EmailInUse;
		}

		if (await _dbContext.Users.AnyAsync(u => u.UserName == user.UserName))
		{
			return RegisterUserResult.UserNameInUse;
		}

		if (user.UserName.Any(c => !UserNameAllowedCharacters.Contains(c)))
		{
			return RegisterUserResult.InvalidUserNameCharacter;
		}

		_ = _dbContext.Users.Add(user);
		_ = await _dbContext.SaveChangesAsync();
		return RegisterUserResult.Success;
	}

	[SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Prefer to use an instance")]
	internal String GenerateEmailConfirmationToken(User user)
	{
		var emailConfirmation = new PendingEmailConfirmation();
		_ = s_pendingEmailConfirmations.AddOrUpdate(user.Id, static (_, e) => e, static (_, _, e) => e, emailConfirmation);
		return emailConfirmation.Token;
	}

	internal async ValueTask<Boolean> ConfirmEmailAsync(User user, String token)
	{
		if (!s_pendingEmailConfirmations.TryGetValue(user.Id, out var emailConfirmation) ||
		    emailConfirmation.Token != token ||
		    DateTime.UtcNow - emailConfirmation.CreationDate > s_pendingEmailConfirmationsLifeTime)
		{
			return false;
		}

		_ = s_pendingEmailConfirmations.TryRemove(user.Id, out _);

		_ = _dbContext.Attach(user);
		user.ConfirmEmail();
		_ = await _dbContext.SaveChangesWithConcurrencyCheckBypassAsync();

		return true;
	}

	internal Boolean CheckPassword(User user, String password)
	{
		if (user.PasswordHash is null)
		{
			throw new InvalidOperationException("The user has no password.");
		}

		var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
		return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
	}

	[SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Prefer to use an instance")]
	internal String GeneratePasswordResetToken(User user)
	{
		var passwordReset = new PendingPasswordReset();
		_ = s_pendingPasswordResets.AddOrUpdate(user.Id, static (_, p) => p, static (_, _, p) => p, passwordReset);
		return passwordReset.Token;
	}

	internal async ValueTask<ResetPasswordResult> ResetPasswordAsync(User user, String newPassword, String token)
	{
		if (!s_pendingPasswordResets.TryGetValue(user.Id, out var passwordReset) ||
		    passwordReset.Token != token ||
		    DateTime.UtcNow - passwordReset.CreationDate > s_pendingPasswordResetsLifeTime)
		{
			return ResetPasswordResult.InvalidToken;
		}

		if (Options.Password.RequireUppercase &&
		    newPassword.Any(c => !UppercaseCharacters.Contains(c)))
		{
			return ResetPasswordResult.MissingUppercaseCharacter;
		}

		if (Options.Password.RequireLowercase &&
		    newPassword.Any(c => !LowercaseCharacters.Contains(c)))
		{
			return ResetPasswordResult.MissingUppercaseCharacter;
		}

		if (Options.Password.RequireDigit &&
		    newPassword.Any(c => !Digits.Contains(c)))
		{
			return ResetPasswordResult.MissingDigit;
		}

		if (newPassword.Length < Options.Password.RequiredLength ||
		    newPassword.Length > User.PasswordMaximumLength)
		{
			return ResetPasswordResult.InvalidLength;
		}

		if (Options.Password.RequireNonAlphanumeric &&
		    newPassword.All(c => LowercaseCharacters.Contains(c) || UppercaseCharacters.Contains(c) || Digits.Contains(c)))
		{
			return ResetPasswordResult.MissingUppercaseCharacter;
		}

		var passwordCharacters = new List<Char>();
		if (Options.Password.RequiredUniqueChars > 0)
		{
			foreach (var character in newPassword)
			{
				if (!passwordCharacters.Contains(character))
				{
					passwordCharacters.Add(character);
				}
			}
		}

		if (passwordCharacters.Count < Options.Password.RequiredUniqueChars)
		{
			return ResetPasswordResult.NotEnoughUniqueCharacters;
		}

		if (!s_pendingPasswordResets.TryRemove(user.Id, out _))
		{
			// Already removed by concurrent reset password operation.
			return ResetPasswordResult.UnknownProblem;
		}

		_ = _dbContext.Attach(user);
		user.ChangePassword(_passwordHasher.HashPassword(user, newPassword));
		_ = await _dbContext.SaveChangesWithConcurrencyCheckBypassAsync();

		return ResetPasswordResult.Success;
	}

	internal async Task AccessFailedAsync(User user)
	{
		_ = _dbContext.Attach(user);

		user.IncrementAccessFailedCount();
		if (user.AccessFailedCount >= Options.Lockout.MaximumFailedAccessAttempts)
		{
			user.Lockout(TimeSpan.FromMinutes(Options.Lockout.LockoutMinutes));
		}

		_ = await _dbContext.SaveChangesWithConcurrencyCheckBypassAsync();
	}

	private sealed class PendingEmailConfirmation
	{
		internal String Token { get; } = Guid.CreateVersion7().ToString("N");

		internal DateTime CreationDate { get; } = DateTime.UtcNow;
	}

	private sealed class PendingPasswordReset
	{
		internal String Token { get; } = Guid.CreateVersion7().ToString("N");

		internal DateTime CreationDate { get; } = DateTime.UtcNow;
	}
}
