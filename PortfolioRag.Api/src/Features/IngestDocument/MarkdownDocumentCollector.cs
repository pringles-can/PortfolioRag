namespace PortfolioRag.Api.Features.IngestDocument;

public sealed class MarkdownDocumentCollector : IDocumentCollector
{
    // Exclude so they don't pollute retrieval.
    private static readonly HashSet<string> ExcludedFileNames =
        new(StringComparer.OrdinalIgnoreCase) { "README.md", "manifest.md" };

    public IReadOnlyList<DocumentFile> Collect(string docsPath)
    {
        if (!Directory.Exists(docsPath))
        {
            return Array.Empty<DocumentFile>();
        }

        return Directory
            .EnumerateFiles(docsPath, "*.md", SearchOption.AllDirectories)
            .Where(path => !ExcludedFileNames.Contains(Path.GetFileName(path)))
            .Select(path => new DocumentFile(
                Source: ToRelativeSource(docsPath, path),
                Path: path,
                Category: ToCategory(docsPath, path)))
            .OrderBy(document => document.Source, StringComparer.Ordinal)
            .ToList();
    }

    private static string ToRelativeSource(string docsPath, string fullPath) =>
        Path.GetRelativePath(docsPath, fullPath).Replace('\\', '/');

    // The document's section: its immediate parent folder name, or empty when
    // the file sits directly in the docs root.
    private static string ToCategory(string docsPath, string fullPath)
    {
        var directory = Path.GetDirectoryName(fullPath);

        if (directory is null ||
            string.Equals(
                Path.GetFullPath(directory),
                Path.GetFullPath(docsPath),
                StringComparison.OrdinalIgnoreCase))
        {
            return string.Empty;
        }

        return Path.GetFileName(directory);
    }
}
