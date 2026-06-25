using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Pgvector;
using PortfolioRag.Api.Features.GenerateEmbedding;
using PortfolioRag.Api.Infrastructure.VectorStore;
using Testcontainers.PostgreSql;
using Xunit;

namespace PortfolioRag.Api.Tests.src.Infrastructure.VectorStore;

[Trait("Category", "Integration")]
public sealed class SearchKnowledgeServiceIntegrationTests : IAsyncLifetime
{
    private const int Dimensions = PortfolioRagDbContext.EmbeddingDimensions;

    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("pgvector/pgvector:pg16")
        .Build();

    private PortfolioRagDbContext _dbContext = null!;

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync();

        var options = new DbContextOptionsBuilder<PortfolioRagDbContext>()
            .UseNpgsql(
                _container.GetConnectionString(),
                npgsql => npgsql.UseVector())
            .Options;

        _dbContext = new PortfolioRagDbContext(options);

        // Applies the real InitialCreate migration: enables the vector
        // extension and creates document_chunks.
        await _dbContext.Database.MigrateAsync();

        await SeedAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _dbContext.DisposeAsync();
        await _container.DisposeAsync();
    }

    [Fact]
    public async Task SearchAsync_ReturnsTheNearestChunk_ForTheQueryEmbedding()
    {
        // Query embedding points straight at the "resume.md" vector.
        var service = new SearchKnowledgeService(
            _dbContext,
            new StubEmbeddingService(Embedding(1f, 0f, 0f)));

        var matches = await service.SearchAsync("anything", topK: 1, CancellationToken.None);

        var match = Assert.Single(matches);
        Assert.Equal("resume.md", match.Source);
        Assert.Equal("dotnet experience", match.Content);
    }

    [Fact]
    public async Task SearchAsync_OrdersByDistance_AndRespectsTopK()
    {
        var service = new SearchKnowledgeService(
            _dbContext,
            new StubEmbeddingService(Embedding(1f, 0f, 0f)));

        var matches = await service.SearchAsync("anything", topK: 2, CancellationToken.None);

        Assert.Equal(2, matches.Count);
        Assert.Equal(
            new[] { "resume.md", "roadmap.md" },
            matches.Select(m => m.Source));
    }

    private async Task SeedAsync()
    {
        _dbContext.DocumentChunks.AddRange(
            NewChunk("resume.md", "dotnet experience", Embedding(1f, 0f, 0f)),
            NewChunk("roadmap.md", "future plans", Embedding(0.8f, 0.6f, 0f)),
            NewChunk("extra.md", "unrelated", Embedding(0.1f, 0f, 0.5f)));

        await _dbContext.SaveChangesAsync();
    }

    private static DocumentChunk NewChunk(string source, string content, Vector embedding) =>
        new()
        {
            Id = Guid.NewGuid(),
            Source = source,
            ChunkIndex = 0,
            Content = content,
            Embedding = embedding,
            CreatedAt = DateTimeOffset.UtcNow,
        };

    // Builds a Dimensions-length vector with the given leading components.
    private static Vector Embedding(params float[] leading)
    {
        var values = new float[Dimensions];
        Array.Copy(leading, values, leading.Length);
        return new Vector(values);
    }

    private sealed class StubEmbeddingService : IEmbeddingService
    {
        private readonly Vector _queryEmbedding;

        public StubEmbeddingService(Vector queryEmbedding) => _queryEmbedding = queryEmbedding;

        public Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken) =>
            Task.FromResult(_queryEmbedding.ToArray());

        public Task<IReadOnlyList<float[]>> EmbedBatchAsync(
            IReadOnlyList<string> texts,
            CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<float[]>>(
                texts.Select(_ => _queryEmbedding.ToArray()).ToList());
    }
}
