using Pgvector;

namespace PortfolioRag.Api.Infrastructure.VectorStore;

public sealed class DocumentChunk
{
    public Guid Id { get; set; }

    /// <summary>Source document the chunk came from, e.g. "resume.md".</summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>Ordinal position of this chunk within its source document.</summary>
    public int ChunkIndex { get; set; }

    public string Content { get; set; } = string.Empty;

    public Vector Embedding { get; set; } = null!;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
