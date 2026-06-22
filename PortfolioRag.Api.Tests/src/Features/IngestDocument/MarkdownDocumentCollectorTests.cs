using System;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using PortfolioRag.Api.Features.IngestDocument;
using Xunit;

namespace PortfolioRag.Api.Tests.src.Features.IngestDocument;

[TestSubject(typeof(MarkdownDocumentCollector))]
public sealed class MarkdownDocumentCollectorTests
{
    private readonly MarkdownDocumentCollector _collector = new();

    [Fact]
    public void Collect_RecursesSubdirectories_AndExcludesNavigationFiles()
    {
        var docsPath = CreateDocs(
            "README.md",
            "manifest.md",
            "resume/summary.md",
            "technologies/kafka.md",
            "interview/architecture.md",
            "accomplishments/architecture.md");

        var sources = _collector.Collect(docsPath).Select(d => d.Source).ToList();

        Assert.DoesNotContain("README.md", sources);
        Assert.DoesNotContain("manifest.md", sources);
        Assert.Equal(4, sources.Count);
        Assert.Contains("resume/summary.md", sources);
        Assert.Contains("technologies/kafka.md", sources);
    }

    [Fact]
    public void Collect_DistinguishesDuplicateFileNamesAcrossFolders()
    {
        var docsPath = CreateDocs(
            "interview/architecture.md",
            "accomplishments/architecture.md");

        var sources = _collector.Collect(docsPath).Select(d => d.Source).ToList();

        Assert.Contains("interview/architecture.md", sources);
        Assert.Contains("accomplishments/architecture.md", sources);
    }

    [Fact]
    public void Collect_SourceUsesForwardSlashes_AndPathPointsToTheFile()
    {
        var docsPath = CreateDocs("technologies/kafka.md");

        var document = Assert.Single(_collector.Collect(docsPath));

        Assert.Equal("technologies/kafka.md", document.Source);
        Assert.DoesNotContain('\\', document.Source);
        Assert.True(File.Exists(document.Path));
    }

    [Fact]
    public void Collect_ExcludesNavigationFiles_CaseInsensitively_AndInSubfolders()
    {
        var docsPath = CreateDocs(
            "section/README.md",
            "section/Manifest.md",
            "section/real.md");

        var sources = _collector.Collect(docsPath).Select(d => d.Source).ToList();

        Assert.Equal(new[] { "section/real.md" }, sources);
    }

    [Fact]
    public void Collect_IgnoresNonMarkdownFiles()
    {
        var docsPath = CreateDocs("notes.txt", "data.json");

        Assert.Empty(_collector.Collect(docsPath));
    }

    [Fact]
    public void Collect_MissingDirectory_ReturnsEmpty()
    {
        var missing = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

        Assert.Empty(_collector.Collect(missing));
    }

    private static string CreateDocs(params string[] relativeFiles)
    {
        var docsPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

        foreach (var relative in relativeFiles)
        {
            var full = Path.Combine(
                docsPath,
                relative.Replace('/', Path.DirectorySeparatorChar));

            Directory.CreateDirectory(Path.GetDirectoryName(full)!);
            File.WriteAllText(full, $"# {relative}");
        }

        return docsPath;
    }
}
