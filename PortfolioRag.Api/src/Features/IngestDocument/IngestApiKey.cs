namespace PortfolioRag.Api.Features.IngestDocument;

public static class IngestApiKey
{
    public const string HeaderName = "X-Ingest-Key";

    /// <summary>
    /// True only when a key is configured and the provided key matches it
    /// exactly. Fails closed when no key is configured.
    /// </summary>
    public static bool IsAuthorized(string? providedKey, string? configuredKey)
    {
        if (string.IsNullOrEmpty(configuredKey))
        {
            return false;
        }

        return string.Equals(providedKey, configuredKey, StringComparison.Ordinal);
    }
}
