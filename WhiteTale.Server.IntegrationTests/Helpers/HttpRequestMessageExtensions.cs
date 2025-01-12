using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace WhiteTale.Server.IntegrationTests.Helpers;

internal static class HttpRequestMessageExtensions
{
	internal static void SetJsonContent<T>(this HttpRequestMessage request, T content)
	{
		var requestBodyString = JsonSerializer.Serialize(content, JsonSerializerOptions.Web);
		request.Content = new StringContent(requestBodyString, Encoding.UTF8, "application/json");
	}

	internal static void SetAuthorization(this HttpRequestMessage request, String scheme, String parameter)
	{
		request.Headers.Authorization = new AuthenticationHeaderValue(scheme, parameter);
	}
}
