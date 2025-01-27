using System.Net;
using System.Net.Http.Json;
using WhiteTale.Server.Domain.Messages;
using WhiteTale.Server.Domain.Users;
using WhiteTale.Server.Features.Rooms.Messages;

namespace WhiteTale.Server.IntegrationTests.Tests.Rooms.Messages;

public sealed class SendMessageTests
{
	[Fact]
	public async Task SendMessage_StandardType_ReturnsOkWithMessage()
	{
		// Arrange
		await using var application = new ApplicationInstance(nameof(SendMessage_StandardType_ReturnsOkWithMessage));
		using var client = application.CreateClient();

		var userSeed = await application.SeedUserAsync(UserSeed.Default with
		{
			Permissions = Permissions.SendMessages,
			CurrentRoomId = 1,
		});
		var userCredentials = await application.LoginUserAsync(userSeed.Seed.UserName, userSeed.Seed.Password);

		var roomSeed = await application.SeedRoomAsync(RoomSeed.Default with
		{
			Id = userSeed.Seed.CurrentRoomId!.Value,
		});

		using var request = new HttpRequestMessage(HttpMethod.Post, $"api/v1/rooms/{roomSeed.Seed.Id}/messages");
		var requestBody = new SendMessageRequestBody
		{
			Type = MessageType.Standard,
			Content = "Hello world",
		};
		request.SetJsonContent(requestBody);
		request.SetAuthorization("Bearer", userCredentials.AccessToken);

		// Act
		var response = await client.SendAsync(request);

		// Assert
		_ = await response.EnsureStatusCodeAsync(HttpStatusCode.OK);
		var responseBody = await response.Content.ReadFromJsonAsync<MessageData>();
		_ = responseBody.Should().NotBeNull();
		_ = responseBody!.Type.Should().Be(requestBody.Type);
		_ = responseBody.Content.Should().Be(requestBody.Content);
		_ = responseBody.Flags.Should().Be(0);
		_ = responseBody.TargetType.Should().Be(MessageTarget.Room);
		_ = responseBody.TargetId.Should().Be(roomSeed.Seed.Id);
		_ = responseBody.AuthorId.Should().Be(userSeed.Seed.Id);
	}

	[Fact]
	public async Task SendMessage_StandardTypeWithMissingContent_ReturnsBadRequest()
	{
		// Arrange
		await using var application = new ApplicationInstance(nameof(SendMessage_StandardTypeWithMissingContent_ReturnsBadRequest));
		using var client = application.CreateClient();

		var userSeed = await application.SeedUserAsync(UserSeed.Default with
		{
			Permissions = Permissions.SendMessages,
			CurrentRoomId = 1,
		});
		var userCredentials = await application.LoginUserAsync(userSeed.Seed.UserName, userSeed.Seed.Password);

		var roomSeed = await application.SeedRoomAsync(RoomSeed.Default with
		{
			Id = userSeed.Seed.CurrentRoomId!.Value,
		});

		using var request = new HttpRequestMessage(HttpMethod.Post, $"api/v1/rooms/{roomSeed.Seed.Id}/messages");
		var requestBody = new SendMessageRequestBody
		{
			Type = MessageType.Standard,
		};
		request.SetJsonContent(requestBody);
		request.SetAuthorization("Bearer", userCredentials.AccessToken);

		// Act
		var response = await client.SendAsync(request);

		// Assert
		_ = await response.EnsureStatusCodeAsync(HttpStatusCode.BadRequest);
	}

	[Fact]
	public async Task SendMessage_StandardTypeWithUnknownFlags_ReturnsOkWithMessage()
	{
		// Arrange
		await using var application = new ApplicationInstance(nameof(SendMessage_StandardTypeWithUnknownFlags_ReturnsOkWithMessage));
		using var client = application.CreateClient();

		var userSeed = await application.SeedUserAsync(UserSeed.Default with
		{
			Permissions = Permissions.SendMessages,
			CurrentRoomId = 1,
		});
		var userCredentials = await application.LoginUserAsync(userSeed.Seed.UserName, userSeed.Seed.Password);

		var roomSeed = await application.SeedRoomAsync(RoomSeed.Default with
		{
			Id = userSeed.Seed.CurrentRoomId!.Value,
		});

		using var request = new HttpRequestMessage(HttpMethod.Post, $"api/v1/rooms/{roomSeed.Seed.Id}/messages");
		var requestBody = new SendMessageRequestBody
		{
			Type = MessageType.Standard,
			Content = "Hello world",
			Flags = (MessageFlags)1,
		};
		request.SetJsonContent(requestBody);
		request.SetAuthorization("Bearer", userCredentials.AccessToken);

		// Act
		var response = await client.SendAsync(request);

		// Assert
		_ = await response.EnsureStatusCodeAsync(HttpStatusCode.OK);
		var responseBody = await response.Content.ReadFromJsonAsync<MessageData>();
		_ = responseBody.Should().NotBeNull();
		_ = responseBody!.Type.Should().Be(requestBody.Type);
		_ = responseBody.Content.Should().Be(requestBody.Content);
		_ = responseBody.Flags.Should().Be(Message.StandardTypeAllowedFlags);
		_ = responseBody.TargetType.Should().Be(MessageTarget.Room);
		_ = responseBody.TargetId.Should().Be(roomSeed.Seed.Id);
		_ = responseBody.AuthorId.Should().Be(userSeed.Seed.Id);
	}

	[Fact]
	public async Task SendMessage_UnknownType_ReturnsBadRequest()
	{
		// Arrange
		await using var application = new ApplicationInstance(nameof(SendMessage_UnknownType_ReturnsBadRequest));
		using var client = application.CreateClient();

		var userSeed = await application.SeedUserAsync(UserSeed.Default with
		{
			Permissions = Permissions.SendMessages,
			CurrentRoomId = 1,
		});
		var userCredentials = await application.LoginUserAsync(userSeed.Seed.UserName, userSeed.Seed.Password);

		var roomSeed = await application.SeedRoomAsync(RoomSeed.Default with
		{
			Id = userSeed.Seed.CurrentRoomId!.Value,
		});

		using var request = new HttpRequestMessage(HttpMethod.Post, $"api/v1/rooms/{roomSeed.Seed.Id}/messages");
		var requestBody = new SendMessageRequestBody
		{
			Type = (MessageType)Int32.MaxValue,
		};
		request.SetJsonContent(requestBody);
		request.SetAuthorization("Bearer", userCredentials.AccessToken);

		// Act
		var response = await client.SendAsync(request);

		// Assert
		_ = await response.EnsureStatusCodeAsync(HttpStatusCode.BadRequest);
	}

	[Fact]
	public async Task SendMessage_StandardTypeToDifferentRoom_ReturnsForbidden()
	{
		// Arrange
		await using var application = new ApplicationInstance(nameof(SendMessage_StandardTypeToDifferentRoom_ReturnsForbidden));
		using var client = application.CreateClient();

		var userSeed = await application.SeedUserAsync(UserSeed.Default with
		{
			Permissions = Permissions.SendMessages,
			CurrentRoomId = 1,
		});
		var userCredentials = await application.LoginUserAsync(userSeed.Seed.UserName, userSeed.Seed.Password);

		var roomSeed = await application.SeedRoomAsync(RoomSeed.Default with
		{
			Id = 2,
		});

		using var request = new HttpRequestMessage(HttpMethod.Post, $"api/v1/rooms/{roomSeed.Seed.Id}/messages");
		var requestBody = new SendMessageRequestBody
		{
			Type = MessageType.Standard,
			Content = "Hello world",
		};
		request.SetJsonContent(requestBody);
		request.SetAuthorization("Bearer", userCredentials.AccessToken);

		// Act
		var response = await client.SendAsync(request);

		// Assert
		_ = await response.EnsureStatusCodeAsync(HttpStatusCode.Forbidden);
	}
}
