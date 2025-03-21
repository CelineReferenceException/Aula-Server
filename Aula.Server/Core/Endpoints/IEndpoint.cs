using Microsoft.AspNetCore.Routing;

namespace Aula.Server.Core.Endpoints;

internal interface IEndpoint
{
	void Build(IEndpointRouteBuilder route);
}
