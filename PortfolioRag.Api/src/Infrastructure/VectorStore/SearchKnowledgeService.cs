using Microsoft.EntityFrameworkCore;
using Pgvector;
using Pgvector.EntityFrameworkCore;
using PortfolioRag.Api.Features.GenerateEmbedding;
using PortfolioRag.Api.Features.SearchKnowledge;

namespace PortfolioRag.Api.Infrastructure.VectorStore;

public sealed class SearchKnowledgeService : ISearchKnowledgeService
{
    private readonly PortfolioRagDbContext _dbContext;
    private readonly IEmbeddingService _embeddingService;

    public SearchKnowledgeService(
        PortfolioRagDbContext dbContext,
        IEmbeddingService embeddingService)
    {
        _dbContext = dbContext;
        _embeddingService = embeddingService;
    }

    public async Task<IReadOnlyList<KnowledgeMatch>> SearchAsync(
        string query,
        int topK,
        CancellationToken cancellationToken)
    {
        var embedding = await _embeddingService.EmbedAsync(query, cancellationToken);
        var queryVector = new Vector(embedding);

        return await _dbContext.DocumentChunks
            .OrderBy(chunk => chunk.Embedding.CosineDistance(queryVector))
            .Take(topK)
            .Select(chunk => new KnowledgeMatch(chunk.Source, chunk.Content))
            .ToListAsync(cancellationToken);
    }
}
