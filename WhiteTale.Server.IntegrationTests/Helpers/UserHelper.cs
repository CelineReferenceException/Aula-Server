using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using WhiteTale.Server.Common.Persistence;
using WhiteTale.Server.Domain.Users;
using WhiteTale.Server.Features.Identity;

namespace WhiteTale.Server.IntegrationTests.Helpers;

internal static class UserHelper
{


	internal static async Task<SeedUserResult> SeedUserAsync(this ApplicationInstance application, UserSeed? userSeed = null)
	{
		using var scope = application.Services.CreateScope();
		var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

		userSeed ??= UserSeed.Default;

		var user = User.Create(userSeed.Id, userSeed.Email, userSeed.UserName, userSeed.DisplayName, UserOwnerType.Standard, userSeed.Permissions);
		user.EmailConfirmed = userSeed.EmailConfirmed;

		_ = await userManager.CreateAsync(user, userSeed.Password);

		return new SeedUserResult
		{
			Seed = userSeed,
			User = user
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
