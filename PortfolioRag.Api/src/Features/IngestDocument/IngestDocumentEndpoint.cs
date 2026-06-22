using Microsoft.Extensions.Options;

namespace PortfolioRag.Api.Features.IngestDocument;

public static class IngestDocumentEndpoint
{
    public static IEndpointRouteBuilder MapIngestDocument(
        this IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/ingest",
                async (
                    IngestDocumentHandler handler,
                    CancellationToken cancellationToken) =>
                {
                    var response = await handler.Handle(cancellationToken);

                    return Results.Ok(response);
                })
            .AddEndpointFilter(RequireIngestApiKey);

        return app;
    }

    private static async ValueTask<object?> RequireIngestApiKey(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var configuredKey = context.HttpContext.RequestServices
            .GetRequiredService<IOptions<IngestionOptions>>()
            .Value.ApiKey;

        var providedKey = context.HttpContext.Request
            .Headers[IngestApiKey.HeaderName]
            .ToString();

        return IngestApiKey.IsAuthorized(providedKey, configuredKey)
            ? await next(context)
            : Results.Unauthorized();
    }
}
