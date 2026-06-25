namespace PortfolioRag.Api.Features.IngestDocument;

public sealed class IngestionOptions
{
    /// <summary>
    /// Shared secret required to call POST /ingest. When unset, the endpoint
    /// fails closed (all requests denied) so it is never accidentally open.
    /// </summary>
    public string ApiKey { get; init; } = string.Empty;
}
