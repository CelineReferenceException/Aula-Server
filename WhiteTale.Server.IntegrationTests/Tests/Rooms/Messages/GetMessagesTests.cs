using System.Net;
using System.Net.Http.Json;
using WhiteTale.Server.Domain.Users;
using WhiteTale.Server.Features.Rooms.Messages;

namespace WhiteTale.Server.IntegrationTests.Tests.Rooms.Messages;

public class GetMessagesTests
{
	[Fact]
	public async Task GetMessages_ValidOperation_ReturnsOkWithMessages()
	{
		// Arrange
		await using var application = new ApplicationInstance(nameof(GetMessages_ValidOperation_ReturnsOkWithMessages));
		using var client = application.CreateClient();

		var userSeed = await application.SeedUserAsync(UserSeed.Default with
		{
			Permissions = Permissions.ReadMessages,
			CurrentRoomId = 1
		});
		var userCredentials = await application.LoginUserAsync(userSeed.Seed.UserName, userSeed.Seed.Password);

		var roomSeed = await application.SeedRoomAsync(RoomSeed.Default with { Id = userSeed.Seed.CurrentRoomId });
		var messageSeed = await application.SeedMessageAsync(MessageSeed.StandardTypeDefault with
		{
			AuthorId = userSeed.Seed.Id,
			TargetId = roomSeed.Seed.Id
		});

		using var request = new HttpRequestMessage(HttpMethod.Get, $"api/rooms/{roomSeed.Seed.Id}/messages");
		request.SetAuthorization("Bearer", userCredentials.AccessToken);

		// Act
		var response = await client.SendAsync(request);

		// Assert
		_ = await response.EnsureStatusCodeAsync(HttpStatusCode.OK);
		var responseBody = await response.Content.ReadFromJsonAsync<MessageData[]>();
		_ = responseBody.Should().NotBeNull();
		_ = responseBody.Should().HaveCount(1);
		_ = responseBody![0].Type.Should().Be(messageSeed.Seed.Type);
		_ = responseBody[0].Content.Should().Be(messageSeed.Seed.Content);
		_ = responseBody[0].Flags.Should().Be(messageSeed.Seed.Flags);
		_ = responseBody[0].Target.Should().Be(messageSeed.Seed.Target);
		_ = responseBody[0].TargetId.Should().Be(messageSeed.Seed.TargetId);
		_ = responseBody[0].AuthorId.Should().Be(messageSeed.Seed.AuthorId);
	}

	[Fact]
	public async Task GetMessages_AfterMessage_ReturnsOkWithMessages()
	{
		// Arrange
		await using var application = new ApplicationInstance(nameof(GetMessages_AfterMessage_ReturnsOkWithMessages));
		using var client = application.CreateClient();

		var userSeed = await application.SeedUserAsync(UserSeed.Default with
		{
			Permissions = Permissions.ReadMessages,
			CurrentRoomId = 1
		});
		var userCredentials = await application.LoginUserAsync(userSeed.Seed.UserName, userSeed.Seed.Password);

		var roomSeed = await application.SeedRoomAsync(RoomSeed.Default with { Id = userSeed.Seed.CurrentRoomId });
		var firstMessageSeed = await application.SeedMessageAsync(MessageSeed.StandardTypeDefault with
		{
			Id = 1,
			AuthorId = userSeed.Seed.Id,
			TargetId = roomSeed.Seed.Id
		});
		var secondMessageSeed = await application.SeedMessageAsync(MessageSeed.StandardTypeDefault with
		{
			Id = 2,
			AuthorId = userSeed.Seed.Id,
			TargetId = roomSeed.Seed.Id
		});

		using var request = new HttpRequestMessage(HttpMethod.Get, $"api/rooms/{roomSeed.Seed.Id}/messages" +
		                                                           $"?{GetMessages.CountQueryParameter}=1" +
		                                                           $"&{GetMessages.AfterQueryParameter}={firstMessageSeed.Seed.Id}");
		request.SetAuthorization("Bearer", userCredentials.AccessToken);

		// Act
		var response = await client.SendAsync(request);

		// Assert
		_ = await response.EnsureStatusCodeAsync(HttpStatusCode.OK);
		var responseBody = await response.Content.ReadFromJsonAsync<MessageData[]>();
		_ = responseBody.Should().NotBeNull();
		_ = responseBody.Should().HaveCount(1);
		_ = responseBody![0].Type.Should().Be(secondMessageSeed.Seed.Type);
		_ = responseBody[0].Content.Should().Be(secondMessageSeed.Seed.Content);
		_ = responseBody[0].Flags.Should().Be(secondMessageSeed.Seed.Flags);
		_ = responseBody[0].Target.Should().Be(secondMessageSeed.Seed.Target);
		_ = responseBody[0].TargetId.Should().Be(secondMessageSeed.Seed.TargetId);
		_ = responseBody[0].AuthorId.Should().Be(secondMessageSeed.Seed.AuthorId);
	}

	[Fact]
	public async Task GetMessages_BeforeMessage_ReturnsOkWithMessages()
	{
		// Arrange
		await using var application = new ApplicationInstance(nameof(GetMessages_BeforeMessage_ReturnsOkWithMessages));
		using var client = application.CreateClient();

		var userSeed = await application.SeedUserAsync(UserSeed.Default with
		{
			Permissions = Permissions.ReadMessages,
			CurrentRoomId = 1
		});
		var userCredentials = await application.LoginUserAsync(userSeed.Seed.UserName, userSeed.Seed.Password);

		var roomSeed = await application.SeedRoomAsync(RoomSeed.Default with { Id = userSeed.Seed.CurrentRoomId });
		var firstMessageSeed = await application.SeedMessageAsync(MessageSeed.StandardTypeDefault with
		{
			Id = 1,
			AuthorId = userSeed.Seed.Id,
			TargetId = roomSeed.Seed.Id
		});
		var secondMessageSeed = await application.SeedMessageAsync(MessageSeed.StandardTypeDefault with
		{
			Id = 2,
			AuthorId = userSeed.Seed.Id,
			TargetId = roomSeed.Seed.Id
		});

		using var request = new HttpRequestMessage(HttpMethod.Get, $"api/rooms/{roomSeed.Seed.Id}/messages" +
		                                                           $"?{GetMessages.CountQueryParameter}=1" +
		                                                           $"&{GetMessages.BeforeQueryParameter}={secondMessageSeed.Seed.Id}");
		request.SetAuthorization("Bearer", userCredentials.AccessToken);

		// Act
		var response = await client.SendAsync(request);

		// Assert
		_ = await response.EnsureStatusCodeAsync(HttpStatusCode.OK);
		var responseBody = await response.Content.ReadFromJsonAsync<MessageData[]>();
		_ = responseBody.Should().NotBeNull();
		_ = responseBody.Should().HaveCount(1);
		_ = responseBody![0].Type.Should().Be(firstMessageSeed.Seed.Type);
		_ = responseBody[0].Content.Should().Be(firstMessageSeed.Seed.Content);
		_ = responseBody[0].Flags.Should().Be(firstMessageSeed.Seed.Flags);
		_ = responseBody[0].Target.Should().Be(firstMessageSeed.Seed.Target);
		_ = responseBody[0].TargetId.Should().Be(firstMessageSeed.Seed.TargetId);
		_ = responseBody[0].AuthorId.Should().Be(firstMessageSeed.Seed.AuthorId);
	}

	[Fact]
	public async Task GetMessages_InDifferentRoom_ReturnsForbidden()
	{
		// Arrange
		await using var application = new ApplicationInstance(nameof(GetMessages_InDifferentRoom_ReturnsForbidden));
		using var client = application.CreateClient();

		var userSeed = await application.SeedUserAsync(UserSeed.Default with
		{
			Permissions = Permissions.ReadMessages,
			CurrentRoomId = 1
		});
		var userCredentials = await application.LoginUserAsync(userSeed.Seed.UserName, userSeed.Seed.Password);

		var roomSeed = await application.SeedRoomAsync(RoomSeed.Default with { Id = 2 });
		var messageSeed = await application.SeedMessageAsync(MessageSeed.StandardTypeDefault with
		{
			AuthorId = userSeed.Seed.Id,
			TargetId = roomSeed.Seed.Id
		});

		using var request = new HttpRequestMessage(HttpMethod.Get, $"api/rooms/{roomSeed.Seed.Id}/messages");
		request.SetAuthorization("Bearer", userCredentials.AccessToken);

		// Act
		var response = await client.SendAsync(request);

		// Assert
		_ = await response.EnsureStatusCodeAsync(HttpStatusCode.Forbidden);
	}
}
