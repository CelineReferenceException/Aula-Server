using Microsoft.AspNetCore.Routing;

namespace Aula.Server.Common.Endpoints;

internal interface IEndpoint
{
	void Build(IEndpointRouteBuilder route);
}
