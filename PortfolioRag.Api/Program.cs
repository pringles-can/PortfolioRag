using Microsoft.EntityFrameworkCore;
using PortfolioRag.Api.Features.AskQuestion;
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

builder.Services.AddSingleton<
    IPortfolioAssistant,
    PortfolioAssistant>();

var app = builder.Build();

app.MapAskQuestion();

app.Run();