using Microsoft.AspNetCore.Routing;

namespace Aula.Server.Common.Endpoints;

internal interface IApiEndpoint
{
	void Build(IEndpointRouteBuilder route);
}
