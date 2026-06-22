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

builder.Services.AddDbContext<PortfolioRagDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("Postgres"),
        npgsql => npgsql.UseVector()));

builder.Services.AddScoped<AskQuestionHandler>();
builder.Services.AddScoped<IngestDocumentHandler>();

builder.Services.AddScoped<
    ISearchKnowledgeService,
    SearchKnowledgeService>();

builder.Services.AddSingleton<
    IPortfolioAssistant,
    PortfolioAssistant>();

builder.Services.AddSingleton<
    IChunkingDocumentService,
    MarkdownChunkingService>();

builder.Services.AddSingleton<
    IEmbeddingService,
    OpenAiEmbeddingService>();

var app = builder.Build();

app.MapAskQuestion();
app.MapIngestDocument();

app.Run();