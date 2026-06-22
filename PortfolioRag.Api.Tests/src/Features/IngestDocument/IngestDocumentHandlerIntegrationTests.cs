using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using PortfolioRag.Api.Features.ChunkDocument;
using PortfolioRag.Api.Features.GenerateEmbedding;
using PortfolioRag.Api.Features.IngestDocument;
using PortfolioRag.Api.Infrastructure.VectorStore;
using Testcontainers.PostgreSql;
using Xunit;

namespace PortfolioRag.Api.Tests.src.Features.IngestDocument;

[Trait("Category", "Integration")]
public sealed class IngestDocumentHandlerIntegrationTests : IAsyncLifetime
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
            .UseNpgsql(_container.GetConnectionString(), npgsql => npgsql.UseVector())
            .Options;

        _dbContext = new PortfolioRagDbContext(options);
        await _dbContext.Database.MigrateAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _dbContext.DisposeAsync();
        await _container.DisposeAsync();
    }

    [Fact]
    public async Task Handle_IngestsNestedDocs_WithRelativeSources_ExcludingNavFiles()
    {
        var contentRoot = CreateContentRoot(new Dictionary<string, string>
        {
            ["docs/README.md"] = "# Readme",
            ["docs/manifest.md"] = "# Manifest",
            ["docs/technologies/kafka.md"] = "# Kafka\n\nEvent streaming.",
            ["docs/interview/architecture.md"] = "# Interview arch\n\nVertical slices.",
            ["docs/accomplishments/architecture.md"] = "# Accomplishment arch\n\nScaled the system.",
        });

        var handler = new IngestDocumentHandler(
            new FakeWebHostEnvironment(contentRoot),
            new MarkdownDocumentCollector(),
            new MarkdownChunkingService(),
            new FakeEmbeddingService(Dimensions),
            _dbContext);

        var response = await handler.Handle(CancellationToken.None);

        Assert.Equal(3, response.FilesProcessed); // README + manifest excluded

        var stored = await _dbContext.DocumentChunks.ToListAsync(TestContext.Current.CancellationToken);

        var sources = stored.Select(c => c.Source).Distinct().ToList();
        Assert.Contains("technologies/kafka.md", sources);
        Assert.Contains("interview/architecture.md", sources);
        Assert.Contains("accomplishments/architecture.md", sources);
        Assert.DoesNotContain(sources, s => s.Contains("manifest", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(sources, s => s.Contains("readme", StringComparison.OrdinalIgnoreCase));

        // Category is derived from the section folder and persisted.
        Assert.Equal(
            "technologies",
            stored.Single(c => c.Source == "technologies/kafka.md").Category);
        Assert.Equal(
            "interview",
            stored.Single(c => c.Source == "interview/architecture.md").Category);
    }

    private static string CreateContentRoot(Dictionary<string, string> files)
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

        foreach (var file in files)
        {
            var full = Path.Combine(root, file.Key.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(full)!);
            File.WriteAllText(full, file.Value);
        }

        return root;
    }

    private sealed class FakeEmbeddingService : IEmbeddingService
    {
        private readonly int _dimensions;

        public FakeEmbeddingService(int dimensions) => _dimensions = dimensions;

        public Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken) =>
            Task.FromResult(new float[_dimensions]);

        public Task<IReadOnlyList<float[]>> EmbedBatchAsync(
            IReadOnlyList<string> texts,
            CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<float[]>>(
                texts.Select(_ => new float[_dimensions]).ToList());
    }

    private sealed class FakeWebHostEnvironment : IWebHostEnvironment
    {
        public FakeWebHostEnvironment(string contentRootPath)
        {
            ContentRootPath = contentRootPath;
            WebRootPath = Path.Combine(contentRootPath, "wwwroot");
        }

        public string ApplicationName { get; set; } = "PortfolioRag.Api.Tests";
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
        public string ContentRootPath { get; set; }
        public string EnvironmentName { get; set; } = "Development";
        public string WebRootPath { get; set; }
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
    }
}
