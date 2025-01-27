using System.Net;
using System.Net.Http.Json;
using WhiteTale.Server.Domain.Users;
using WhiteTale.Server.Features.Rooms.Messages;

namespace WhiteTale.Server.IntegrationTests.Tests.Rooms.Messages;

public sealed class RemoveMessageTests
{
	[Fact]
	public async Task RemoveMessage_RemoveOwnedMessage_ReturnsOkWithMessage()
	{
		// Arrange
		await using var application = new ApplicationInstance(nameof(RemoveMessage_RemoveOwnedMessage_ReturnsOkWithMessage));
		using var client = application.CreateClient();

		var userSeed = await application.SeedUserAsync();
		var userCredentials = await application.LoginUserAsync(userSeed.Seed.UserName, userSeed.Seed.Password);

		var roomSeed = await application.SeedRoomAsync();
		var messageSeed = await application.SeedMessageAsync(MessageSeed.StandardTypeDefault with
		{
			AuthorId = userSeed.Seed.Id,
			TargetId = roomSeed.Seed.Id,
		});

		using var request = new HttpRequestMessage(HttpMethod.Delete, $"api/v1/rooms/{roomSeed.Seed.Id}/messages/{messageSeed.Seed.Id}");
		request.SetAuthorization("Bearer", userCredentials.AccessToken);

		// Act
		var response = await client.SendAsync(request);

		// Assert
		_ = await response.EnsureStatusCodeAsync(HttpStatusCode.OK);
		var responseBody = await response.Content.ReadFromJsonAsync<MessageData>();
		_ = responseBody.Should().NotBeNull();
		_ = responseBody!.Type.Should().Be(messageSeed.Seed.Type);
		_ = responseBody.Content.Should().Be(messageSeed.Seed.Content);
		_ = responseBody.Flags.Should().Be(messageSeed.Seed.Flags);
		_ = responseBody.TargetType.Should().Be(messageSeed.Seed.Target);
		_ = responseBody.TargetId.Should().Be(messageSeed.Seed.TargetId);
		_ = responseBody.AuthorId.Should().Be(messageSeed.Seed.AuthorId);
	}

	[Fact]
	public async Task RemoveMessage_RemoveNotOwnedMessageWithoutPermissions_ReturnsForbidden()
	{
		// Arrange
		await using var application =
			new ApplicationInstance(nameof(RemoveMessage_RemoveNotOwnedMessageWithoutPermissions_ReturnsForbidden));
		using var client = application.CreateClient();

		var userSeed = await application.SeedUserAsync(UserSeed.Default with
		{
			Id = 1,
		});
		var userCredentials = await application.LoginUserAsync(userSeed.Seed.UserName, userSeed.Seed.Password);

		var roomSeed = await application.SeedRoomAsync();
		var messageSeed = await application.SeedMessageAsync(MessageSeed.StandardTypeDefault with
		{
			AuthorId = 2,
			TargetId = roomSeed.Seed.Id,
		});

		using var request = new HttpRequestMessage(HttpMethod.Delete, $"api/v1/rooms/{roomSeed.Seed.Id}/messages/{messageSeed.Seed.Id}");
		request.SetAuthorization("Bearer", userCredentials.AccessToken);

		// Act
		var response = await client.SendAsync(request);

		// Assert
		_ = await response.EnsureStatusCodeAsync(HttpStatusCode.Forbidden);
	}

	[Fact]
	public async Task RemoveMessage_RemoveNotOwnedMessageWithPermissions_ReturnsOkWithMessage()
	{
		// Arrange
		await using var application =
			new ApplicationInstance(nameof(RemoveMessage_RemoveNotOwnedMessageWithPermissions_ReturnsOkWithMessage));
		using var client = application.CreateClient();

		var userSeed = await application.SeedUserAsync(UserSeed.Default with
		{
			Id = 1,
			Permissions = Permissions.ManageMessages,
		});
		var userCredentials = await application.LoginUserAsync(userSeed.Seed.UserName, userSeed.Seed.Password);

		var roomSeed = await application.SeedRoomAsync();
		var messageSeed = await application.SeedMessageAsync(MessageSeed.StandardTypeDefault with
		{
			AuthorId = 2,
			TargetId = roomSeed.Seed.Id,
		});

		using var request = new HttpRequestMessage(HttpMethod.Delete, $"api/v1/rooms/{roomSeed.Seed.Id}/messages/{messageSeed.Seed.Id}");
		request.SetAuthorization("Bearer", userCredentials.AccessToken);

		// Act
		var response = await client.SendAsync(request);

		// Assert
		_ = await response.EnsureStatusCodeAsync(HttpStatusCode.OK);
		var responseBody = await response.Content.ReadFromJsonAsync<MessageData>();
		_ = responseBody.Should().NotBeNull();
		_ = responseBody!.Type.Should().Be(messageSeed.Seed.Type);
		_ = responseBody.Content.Should().Be(messageSeed.Seed.Content);
		_ = responseBody.Flags.Should().Be(messageSeed.Seed.Flags);
		_ = responseBody.TargetType.Should().Be(messageSeed.Seed.Target);
		_ = responseBody.TargetId.Should().Be(messageSeed.Seed.TargetId);
		_ = responseBody.AuthorId.Should().Be(messageSeed.Seed.AuthorId);
	}
}
