using Microsoft.EntityFrameworkCore;
using Pgvector;
using PortfolioRag.Api.Features.ChunkDocument;
using PortfolioRag.Api.Features.GenerateEmbedding;
using PortfolioRag.Api.Infrastructure.VectorStore;

namespace PortfolioRag.Api.Features.IngestDocument;

public sealed class IngestDocumentHandler
{
    private readonly IWebHostEnvironment _environment;
    private readonly IDocumentCollector _documentCollector;
    private readonly IChunkingDocumentService _chunkingService;
    private readonly IEmbeddingService _embeddingService;
    private readonly PortfolioRagDbContext _dbContext;

    public IngestDocumentHandler(
        IWebHostEnvironment environment,
        IDocumentCollector documentCollector,
        IChunkingDocumentService chunkingService,
        IEmbeddingService embeddingService,
        PortfolioRagDbContext dbContext)
    {
        _environment = environment;
        _documentCollector = documentCollector;
        _chunkingService = chunkingService;
        _embeddingService = embeddingService;
        _dbContext = dbContext;
    }

    public async Task<IngestDocumentResponse> Handle(
        CancellationToken cancellationToken)
    {
        var docsPath = Path.Combine(
            _environment.ContentRootPath,
            "docs");

        var documents = _documentCollector.Collect(docsPath);

        // Re-ingestion is a full rebuild: drop existing chunks first so the
        // store always mirrors the current contents of /docs.
        await _dbContext.DocumentChunks.ExecuteDeleteAsync(cancellationToken);

        var totalChunks = 0;

        foreach (var document in documents)
        {
            var text = await File.ReadAllTextAsync(document.Path, cancellationToken);

            var chunks = _chunkingService.Chunk(text);

            if (chunks.Count == 0)
            {
                continue;
            }

            var embeddings = await _embeddingService.EmbedBatchAsync(
                chunks,
                cancellationToken);

            for (var index = 0; index < chunks.Count; index++)
            {
                _dbContext.DocumentChunks.Add(new DocumentChunk
                {
                    Id = Guid.NewGuid(),
                    Source = document.Source,
                    ChunkIndex = index,
                    Content = chunks[index],
                    Embedding = new Vector(embeddings[index]),
                    CreatedAt = DateTimeOffset.UtcNow,
                });
            }

            totalChunks += chunks.Count;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new IngestDocumentResponse(
            FilesProcessed: documents.Count,
            ChunksCreated: totalChunks);
    }
}
