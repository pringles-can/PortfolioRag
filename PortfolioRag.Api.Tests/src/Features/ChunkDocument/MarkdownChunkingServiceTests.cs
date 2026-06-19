using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using PortfolioRag.Api.Features.ChunkDocument;
using Xunit;

namespace PortfolioRag.Api.Tests.src.Features.ChunkDocument;

[TestSubject(typeof(MarkdownChunkingService))]
public sealed class MarkdownChunkingServiceTests
{
    private const int MaxChunkSize = 1000;

    private readonly MarkdownChunkingService _service = new();

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\n\n\t  \n")]
    [InlineData(null)]
    public void Chunk_BlankInput_ReturnsEmpty(string text)
    {
        var chunks = _service.Chunk(text);

        Assert.Empty(chunks);
    }

    [Fact]
    public void Chunk_SingleShortParagraph_ReturnsOneTrimmedChunk()
    {
        var chunks = _service.Chunk("  Steve has .NET experience.  ");

        var chunk = Assert.Single(chunks);
        Assert.Equal("Steve has .NET experience.", chunk);
    }

    [Fact]
    public void Chunk_MultipleParagraphsWithinLimit_AreCombinedIntoOneChunk()
    {
        var text = "First paragraph.\n\nSecond paragraph.\n\nThird paragraph.";

        var chunk = Assert.Single(_service.Chunk(text));

        Assert.Contains("First paragraph.", chunk);
        Assert.Contains("Second paragraph.", chunk);
        Assert.Contains("Third paragraph.", chunk);
    }

    [Fact]
    public void Chunk_ContentExceedingLimit_ProducesMultipleChunks()
    {
        var text = BuildParagraphs(count: 10, paragraphLength: 250);

        var chunks = _service.Chunk(text);

        Assert.True(
            chunks.Count > 1,
            $"Expected multiple chunks for {text.Length}-char input, got {chunks.Count}.");
    }

    [Fact]
    public void Chunk_PreservesEveryParagraphsContent()
    {
        var text = BuildParagraphs(count: 10, paragraphLength: 250);

        var chunks = _service.Chunk(text);
        var combined = string.Join("\n", chunks);

        for (var i = 0; i < 10; i++)
        {
            Assert.Contains($"Paragraph{i}:", combined);
        }
    }

    [Fact]
    public void Chunk_ConsecutiveChunksShareOverlap()
    {
        var text = BuildParagraphs(count: 10, paragraphLength: 250);

        var chunks = _service.Chunk(text);

        Assert.True(chunks.Count > 1, "Test needs at least two chunks to assert overlap.");

        for (var i = 0; i < chunks.Count - 1; i++)
        {
            var nextHead = chunks[i + 1][..Math.Min(30, chunks[i + 1].Length)];

            Assert.Contains(
                nextHead,
                chunks[i]);
        }
    }

    [Fact]
    public void Chunk_KeepsChunksNearTheConfiguredLimit()
    {
        // Paragraphs are individually well under the limit, so no single
        // paragraph can blow a chunk far past MaxChunkSize.
        var text = BuildParagraphs(count: 20, paragraphLength: 200);

        var chunks = _service.Chunk(text);

        Assert.All(
            chunks,
            chunk => Assert.True(
                chunk.Length <= MaxChunkSize + 200,
                $"Chunk length {chunk.Length} exceeded the expected bound."));
    }

    private static string BuildParagraphs(int count, int paragraphLength)
    {
        var paragraphs = Enumerable
            .Range(0, count)
            .Select(i => $"Paragraph{i}: " + new string('x', paragraphLength));

        return string.Join("\n\n", paragraphs);
    }
}
