using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using WhiteTale.Server.Common.Persistence;
using WhiteTale.Server.Domain.Characters;
using WhiteTale.Server.Domain.Users;
using WhiteTale.Server.Features.Identity;

namespace WhiteTale.Server.IntegrationTests.Helpers;

internal static class UserHelper
{
	internal static UserSeed DefaultUserSeed { get; } = new()
	{
		Id = 1,
		DisplayName = "TestUser",
		UserName = "test_user",
		Password = "TestPassword1!",
		Email = "test_address@example.com",
		EmailConfirmed = true
	};

	internal static async Task<SeedUserResult> SeedUserAsync(this ApplicationInstance application, UserSeed? userSeed = null)
	{
		using var scope = application.Services.CreateScope();
		var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
		var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

		userSeed ??= DefaultUserSeed;

		var user = new User(userSeed.UserName)
		{
			Id = userSeed.Id,
			Email = userSeed.Email,
			EmailConfirmed = userSeed.EmailConfirmed,
			Permissions = userSeed.Permissions
		};

		var character = new Character
		{
			Id = userSeed.Id,
			DisplayName = userSeed.DisplayName ?? userSeed.UserName,
			OwnerType = CharacterOwnerType.Standard,
			CreationTime = DateTime.UtcNow,
			ConcurrencyStamp = Guid.NewGuid().ToString()
		};

		_ = await userManager.CreateAsync(user, userSeed.Password);
		_ = dbContext.Characters.Add(character);
		_ = await dbContext.SaveChangesAsync();

		return new SeedUserResult
		{
			Seed = userSeed,
			User = user,
			Character = character
		};
	}

	internal static async Task<AccessTokenResponse> LoginUserAsync(this ApplicationInstance application, String userName, String password)
	{
		var httpClient = application.CreateClient();
		var requestBody = new LogInRequestBody
		{
			UserName = userName,
			Password = password
		};

		using var response = await httpClient.PostAsJsonAsync("api/identity/login", requestBody);
		_ = await response.EnsureStatusCodeAsync(HttpStatusCode.OK);
		return await response.Content.ReadFromJsonAsync<AccessTokenResponse>() ?? throw new UnreachableException();
	}
}
