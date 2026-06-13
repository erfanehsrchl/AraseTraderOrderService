using Api.GrpcServices;

namespace Api.Extensions;

public static class GrpcEndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapApiGrpcServices(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGrpcService<OrderGrpcService>();
        endpoints.MapGrpcService<WalletGrpcService>();

        return endpoints;
    }
}
