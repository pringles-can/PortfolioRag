namespace PortfolioRag.Api.Features.AskQuestion;

public static class AskQuestionEndpoint
{
    public const string RateLimitPolicy = "ask";

    public static IEndpointRouteBuilder MapAskQuestion(
        this IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/ask",
                async (
                    AskQuestionRequest request,
                    AskQuestionHandler handler,
                    CancellationToken cancellationToken) =>
                {
                    var response = await handler.Handle(
                        request,
                        cancellationToken);

                    return Results.Ok(response);
                })
            .RequireRateLimiting(RateLimitPolicy);

        return app;
    }
}