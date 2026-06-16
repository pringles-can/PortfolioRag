using OpenAI.Chat;

namespace PortfolioRag.Api.Infrastructure.OpenAI;

using Microsoft.Extensions.Options;


public sealed class PortfolioAssistant : IPortfolioAssistant
{
    private readonly ChatClient _chatClient;

    public PortfolioAssistant(
        IOptions<OpenAiOptions> options)
    {
        _chatClient = new ChatClient(
            options.Value.Model,
            options.Value.ApiKey);
    }
    
    public async Task<string> AnswerAsync(
        string question,
        string context,
        CancellationToken cancellationToken)
    {
        var prompt = $"""
                      You are answering questions about Steve's portfolio.

                      Answer only from the provided context.

                      If the answer is not present in the context, respond:

                      I don't know based on the available portfolio content.

                      Context:
                      -------------------
                      {context}
                      -------------------

                      Question:
                      {question}
                      """;
        
        var completion = await _chatClient.CompleteChatAsync([prompt],
            null,
            cancellationToken);

        return completion.Value.Content[0].Text;
    }
}
