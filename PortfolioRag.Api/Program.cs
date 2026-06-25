using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using PortfolioRag.Api.Features.AskQuestion;
using PortfolioRag.Api.Features.ChunkDocument;
using PortfolioRag.Api.Features.GenerateEmbedding;
using PortfolioRag.Api.Features.IngestDocument;
using PortfolioRag.Api.Features.SearchKnowledge;
using PortfolioRag.Api.Infrastructure;
using PortfolioRag.Api.Infrastructure.OpenAI;
using PortfolioRag.Api.Infrastructure.VectorStore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<OpenAiOptions>(
    builder.Configuration.GetSection("OpenAI"));

builder.Services.Configure<IngestionOptions>(
    builder.Configuration.GetSection("Ingestion"));

builder.Services.AddDbContext<PortfolioRagDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("Postgres"),
        npgsql => npgsql.UseVector()));

builder.Services.AddHealthChecks();

builder.Services.AddScoped<AskQuestionHandler>();
builder.Services.AddScoped<IngestDocumentHandler>();

builder.Services.AddScoped<
    ISearchKnowledgeService,
    SearchKnowledgeService>();

builder.Services.AddSingleton<
    IPortfolioAssistant,
    PortfolioAssistant>();

builder.Services.AddSingleton<
    IDocumentCollector,
    MarkdownDocumentCollector>();

builder.Services.AddSingleton<
    IChunkingDocumentService,
    MarkdownChunkingService>();

builder.Services.AddSingleton<
    IEmbeddingService,
    OpenAiEmbeddingService>();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter(AskQuestionEndpoint.RateLimitPolicy, o =>
    {
        o.Window      = TimeSpan.FromMinutes(1);
        // Total /ask calls per minute. Tunable via RateLimiting__AskPerMinute
        // (env var) without a code change; defaults to 10.
        o.PermitLimit = builder.Configuration.GetValue<int?>("RateLimiting:AskPerMinute") ?? 10;
        o.QueueLimit  = 0;    // reject immediately past limit
    });
});

var app = builder.Build();

app.UseRateLimiter();

// Apply pending migrations on startup when enabled (single-replica staging).
// Keep disabled for multi-replica deploys to avoid migration races; run them
// as a dedicated deploy step instead.
if (app.Configuration.GetValue<bool>("Database:MigrateOnStartup"))
{
    using var scope = app.Services.CreateScope();
    scope.ServiceProvider
        .GetRequiredService<PortfolioRagDbContext>()
        .Database
        .Migrate();
}

app.MapHealthChecks("/health");

app.MapAskQuestion();
app.MapIngestDocument();

app.Run();