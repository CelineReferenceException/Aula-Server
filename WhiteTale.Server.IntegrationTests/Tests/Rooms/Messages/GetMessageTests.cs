using System.Net;
using System.Net.Http.Json;
using WhiteTale.Server.Domain.Users;
using WhiteTale.Server.Features.Rooms.Messages;

namespace WhiteTale.Server.IntegrationTests.Tests.Rooms.Messages;

public sealed class GetMessageTests
{
	[Fact]
	public async Task GetMessage_ValidOperation_ReturnsOkWithMessage()
	{
		// Arrange
		await using var application = new ApplicationInstance(nameof(GetMessage_ValidOperation_ReturnsOkWithMessage));
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

		using var request = new HttpRequestMessage(HttpMethod.Get, $"api/rooms/{roomSeed.Seed.Id}/messages/{messageSeed.Seed.Id}");
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
		_ = responseBody.Target.Should().Be(messageSeed.Seed.Target);
		_ = responseBody.TargetId.Should().Be(messageSeed.Seed.TargetId);
		_ = responseBody.AuthorId.Should().Be(messageSeed.Seed.AuthorId);
	}

	[Fact]
	public async Task GetMessage_InDifferentRoom_ReturnsForbidden()
	{
		// Arrange
		await using var application = new ApplicationInstance(nameof(GetMessage_InDifferentRoom_ReturnsForbidden));
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

		using var request = new HttpRequestMessage(HttpMethod.Get, $"api/rooms/{roomSeed.Seed.Id}/messages/{messageSeed.Seed.Id}");
		request.SetAuthorization("Bearer", userCredentials.AccessToken);


		// Act
		var response = await client.SendAsync(request);

		// Assert
		_ = await response.EnsureStatusCodeAsync(HttpStatusCode.Forbidden);
	}
}
