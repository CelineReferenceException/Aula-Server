namespace WhiteTale.Server.Common.Endpoints;

internal interface IEndpoint
{
	void Build(IEndpointRouteBuilder route);
}
