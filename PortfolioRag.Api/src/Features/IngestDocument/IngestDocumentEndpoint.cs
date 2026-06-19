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
            });

        return app;
    }
}
