namespace PortfolioRag.Api.Features.ChunkDocument;

public interface IChunkingDocumentService
{
    /// <summary>
    /// Splits a document's text into overlapping chunks suitable for embedding.
    /// </summary>
    IReadOnlyList<string> Chunk(string text);
}
