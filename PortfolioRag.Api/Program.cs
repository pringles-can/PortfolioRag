using PortfolioRag.Api.Features.AskQuestion;
using PortfolioRag.Api.Infrastructure;
using PortfolioRag.Api.Infrastructure.OpenAI;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<OpenAiOptions>(
    builder.Configuration.GetSection("OpenAI"));

builder.Services.AddScoped<AskQuestionHandler>();

builder.Services.AddSingleton<
    IPortfolioAssistant,
    PortfolioAssistant>();

var app = builder.Build();

app.MapAskQuestion();

app.Run();