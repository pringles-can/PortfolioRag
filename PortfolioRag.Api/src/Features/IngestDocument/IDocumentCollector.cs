namespace PortfolioRag.Api.Features.IngestDocument;

public interface IDocumentCollector
{
    /// <summary>
    /// Recursively finds markdown documents under <paramref name="docsPath"/>,
    /// excluding navigation files (README.md, manifest.md). Each document's
    /// Source is its path relative to docsPath, using forward slashes, so that
    /// same-named files in different folders stay distinct.
    /// </summary>
    IReadOnlyList<DocumentFile> Collect(string docsPath);
}
