using System.Net;
using System.Net.Http.Json;
using WhiteTale.Server.Features.Identity;
using WhiteTale.Server.Features.Identity.Endpoints;

namespace WhiteTale.Server.IntegrationTests.Tests.Identity;

public sealed class RegisterTest
{
	[Fact]
	public async Task Register_WithValidCredentials_ReturnsNoContent()
	{
		// Arrange
		await using var application = new ApplicationInstance(nameof(Register_WithValidCredentials_ReturnsNoContent));
		using var httpClient = application.CreateClient();

		var requestBody = new RegisterRequestBody
		{
			UserName = "test_user",
			Password = "TestPassword1!",
			Email = "test_address@example.com",
		};

		// Act
		using var response = await httpClient.PostAsJsonAsync("api/v1/identity/register", requestBody);

		// Assert
		_ = await response.EnsureStatusCodeAsync(HttpStatusCode.NoContent);
	}

	[Fact]
	public async Task Register_WithEmptyBody_ReturnsBadRequest()
	{
		// Arrange
		await using var application = new ApplicationInstance(nameof(Register_WithEmptyBody_ReturnsBadRequest));
		using var httpClient = application.CreateClient();

		// Act
		using var response = await httpClient.PostAsJsonAsync("api/v1/identity/register", String.Empty);

		// Assert
		_ = await response.EnsureStatusCodeAsync(HttpStatusCode.BadRequest);
	}

	[Fact]
	public async Task Register_TwoUsersWithSameEmail_ReturnsNoContent()
	{
		// Arrange
		await using var application = new ApplicationInstance(nameof(Register_TwoUsersWithSameEmail_ReturnsNoContent));
		using var httpClient = application.CreateClient();
		var firstRequestBody = new RegisterRequestBody
		{
			UserName = "first_test_user",
			Password = "TestPassword1!",
			Email = "test_address@example.com",
		};
		var secondRequestBody = new RegisterRequestBody
		{
			UserName = "second_test_user",
			Password = "TestPassword1!",
			Email = firstRequestBody.Email,
		};

		using var firstResponse = await httpClient.PostAsJsonAsync("api/v1/identity/register", firstRequestBody);

		// Act
		using var secondResponse = await httpClient.PostAsJsonAsync("api/v1/identity/register", secondRequestBody);

		// Assert
		_ = await firstResponse.EnsureStatusCodeAsync(HttpStatusCode.NoContent);
		_ = await secondResponse.EnsureStatusCodeAsync(HttpStatusCode.NoContent);
	}

	[Fact]
	public async Task Register_TwoUsersWithSameUserName_ReturnsBadRequest()
	{
		// Arrange
		await using var application = new ApplicationInstance(nameof(Register_TwoUsersWithSameUserName_ReturnsBadRequest));
		using var httpClient = application.CreateClient();
		var firstRequestBody = new RegisterRequestBody
		{
			UserName = "test_user",
			Password = "TestPassword1!",
			Email = "testa_ddress_1@example.com",
		};
		var secondRequestBody = new RegisterRequestBody
		{
			UserName = firstRequestBody.UserName,
			Password = "TestPassword1!",
			Email = "testa_ddress_2@example.com",
		};

		using var firstResponse = await httpClient.PostAsJsonAsync("api/v1/identity/register", firstRequestBody);

		// Act
		using var secondResponse = await httpClient.PostAsJsonAsync("api/v1/identity/register", secondRequestBody);

		// Assert
		_ = await firstResponse.EnsureStatusCodeAsync(HttpStatusCode.NoContent);
		_ = await secondResponse.EnsureStatusCodeAsync(HttpStatusCode.BadRequest);
	}
}
