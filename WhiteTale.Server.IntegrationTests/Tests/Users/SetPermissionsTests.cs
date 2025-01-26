using System.Net;
using WhiteTale.Server.Domain.Users;
using WhiteTale.Server.Features.Users;

namespace WhiteTale.Server.IntegrationTests.Tests.Users;

public sealed class SetPermissionsTests
{
	[Fact]
	public async Task SetPermissions_ValidOperation_ReturnsNoContent()
	{
		// Arrange
		await using var application = new ApplicationInstance(nameof(SetPermissions_ValidOperation_ReturnsNoContent));
		using var httpClient = application.CreateClient();

		var executorUser = await application.SeedUserAsync(UserSeed.Default with
		{
			Id = 1,
			UserName = "test_user_x",
			Email = "test_address_1@example.com",
			Permissions = Permissions.Administrator,
		});
		var userToModify = await application.SeedUserAsync(UserSeed.Default with
		{
			Id = 2,
			UserName = "test_user_y",
			Email = "test_address_2@example.com",
		});
		var credentials = await application.LoginUserAsync(executorUser.Seed.UserName, executorUser.Seed.Password);

		using var request = new HttpRequestMessage(HttpMethod.Put, $"api/v1/users/{userToModify.Seed.Id}/permissions");
		var requestBody = new SetPermissionsRequestBody
		{
			Permissions = 0,
		};
		request.SetJsonContent(requestBody);
		request.SetAuthorization("Bearer", credentials.AccessToken);

		// Act
		using var response = await httpClient.SendAsync(request);

		// Assert
		_ = await response.EnsureStatusCodeAsync(HttpStatusCode.NoContent);
	}

	[Fact]
	public async Task SetPermissions_WithMissingPermissions_ReturnsForbidden()
	{
		// Arrange
		await using var application = new ApplicationInstance(nameof(SetPermissions_WithMissingPermissions_ReturnsForbidden));
		using var httpClient = application.CreateClient();

		var executorUser = await application.SeedUserAsync(UserSeed.Default with
		{
			Id = 1,
			UserName = "test_user_x",
			Email = "test_address_1@example.com",
		});
		var userToModify = await application.SeedUserAsync(UserSeed.Default with
		{
			Id = 2,
			UserName = "test_user_y",
			Email = "test_address_2@example.com",
		});
		var credentials = await application.LoginUserAsync(executorUser.Seed.UserName, executorUser.Seed.Password);

		using var request = new HttpRequestMessage(HttpMethod.Put, $"api/v1/users/{userToModify.Seed.Id}/permissions");
		var requestBody = new SetPermissionsRequestBody
		{
			Permissions = 0,
		};
		request.SetJsonContent(requestBody);
		request.SetAuthorization("Bearer", credentials.AccessToken);

		// Act
		using var response = await httpClient.SendAsync(request);

		// Assert
		_ = await response.EnsureStatusCodeAsync(HttpStatusCode.Forbidden);
	}

	[Fact]
	public async Task SetPermissions_SetAdministrator_ReturnsBadRequest()
	{
		// Arrange
		await using var application = new ApplicationInstance(nameof(SetPermissions_SetAdministrator_ReturnsBadRequest));
		using var httpClient = application.CreateClient();

		var executorUser = await application.SeedUserAsync(UserSeed.Default with
		{
			Id = 1,
			UserName = "test_user_x",
			Email = "test_address_1@example.com",
			Permissions = Permissions.Administrator,
		});
		var userToModify = await application.SeedUserAsync(UserSeed.Default with
		{
			Id = 2,
			UserName = "test_user_y",
			Email = "test_address_2@example.com",
		});
		var credentials = await application.LoginUserAsync(executorUser.Seed.UserName, executorUser.Seed.Password);

		using var request = new HttpRequestMessage(HttpMethod.Put, $"api/v1/users/{userToModify.Seed.Id}/permissions");
		var requestBody = new SetPermissionsRequestBody
		{
			Permissions = Permissions.Administrator,
		};
		request.SetJsonContent(requestBody);
		request.SetAuthorization("Bearer", credentials.AccessToken);

		// Act
		using var response = await httpClient.SendAsync(request);

		// Assert
		_ = await response.EnsureStatusCodeAsync(HttpStatusCode.BadRequest);
	}
}
