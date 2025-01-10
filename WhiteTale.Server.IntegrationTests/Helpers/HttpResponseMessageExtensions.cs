using System.Net;

namespace WhiteTale.Server.IntegrationTests.Helpers;

internal static class HttpResponseMessageExtensions
{
	internal static async ValueTask<HttpResponseMessage> EnsureStatusCodeAsync(
		this HttpResponseMessage response,
		HttpStatusCode expectedStatusCode)
	{
		if (response.StatusCode != expectedStatusCode)
		{
			var content = await response.Content.ReadAsStringAsync();
			throw new HttpRequestException(
				$"Unexpected status code {(Int32)response.StatusCode} '{response.StatusCode}' with content: '{content}'", null,
				response.StatusCode);
		}

		return response;
	}
}
