namespace PortfolioRag.Api.Features.IngestDocument;

/// <summary>
/// A markdown document to ingest. <paramref name="Source"/> is the path
/// relative to the docs root (forward slashes); <paramref name="Path"/> is
/// the absolute file path; <paramref name="Category"/> is the section folder
/// the document lives in (its immediate parent folder name), or empty if it
/// sits directly in the docs root.
/// </summary>
public sealed record DocumentFile(string Source, string Path, string Category);
