using System.Text;

namespace PortfolioRag.Api.Features.ChunkDocument;

public sealed class MarkdownChunkingService : IChunkingDocumentService
{
    private const int MaxChunkSize = 1000;
    private const int ChunkOverlap = 200;

    public IReadOnlyList<string> Chunk(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return Array.Empty<string>();
        }

        var paragraphs = text
            .Replace("\r\n", "\n")
            .Split(
                "\n\n",
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var chunks = new List<string>();
        var current = new StringBuilder();

        foreach (var paragraph in paragraphs)
        {
            var wouldOverflow =
                current.Length > 0 &&
                current.Length + paragraph.Length > MaxChunkSize;

            if (wouldOverflow)
            {
                chunks.Add(current.ToString().Trim());
                current = StartNextChunk(current.ToString());
            }

            current.Append(paragraph).Append("\n\n");
        }

        if (current.Length > 0)
        {
            chunks.Add(current.ToString().Trim());
        }

        return chunks;
    }

    // Seeds the next chunk with a tail slice of the previous one so context
    // isn't lost at chunk boundaries.
    private static StringBuilder StartNextChunk(string previous)
    {
        var overlap = previous.Length <= ChunkOverlap
            ? previous
            : previous[^ChunkOverlap..];

        return new StringBuilder(overlap.TrimStart()).Append("\n\n");
    }
}
