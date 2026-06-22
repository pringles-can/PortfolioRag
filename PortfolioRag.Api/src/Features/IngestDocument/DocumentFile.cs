namespace PortfolioRag.Api.Features.IngestDocument;

/// <summary>
/// A markdown document to ingest. <paramref name="Source"/> is the path
/// relative to the docs root (forward slashes); <paramref name="Path"/> is
/// the absolute file path.
/// </summary>
public sealed record DocumentFile(string Source, string Path);
