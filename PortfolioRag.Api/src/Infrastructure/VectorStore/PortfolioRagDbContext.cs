using Microsoft.EntityFrameworkCore;

namespace PortfolioRag.Api.Infrastructure.VectorStore;

public sealed class PortfolioRagDbContext : DbContext
{
    /// <summary>
    /// Embedding width. Matches OpenAI text-embedding-3-small (1536).
    /// WARNING: Changing this requires a new migration and re-embedding existing rows.
    /// </summary>
    public const int EmbeddingDimensions = 1536;

    public PortfolioRagDbContext(DbContextOptions<PortfolioRagDbContext> options)
        : base(options)
    {
    }

    public DbSet<DocumentChunk> DocumentChunks => Set<DocumentChunk>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("vector");

        modelBuilder.Entity<DocumentChunk>(entity =>
        {
            entity.ToTable("document_chunks");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Source).IsRequired();
            entity.Property(x => x.Content).IsRequired();

            entity.Property(x => x.Category)
                .IsRequired()
                .HasMaxLength(256);

            entity.HasIndex(x => x.Category);

            entity.Property(x => x.Embedding)
                .HasColumnType($"vector({EmbeddingDimensions})");
        });
    }
}
