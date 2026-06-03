using Anthropic;
using HeadBet.Core.Domain.Enums;
using HeadBet.Core.Domain.Interfaces;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Chat;

namespace HeadBet.Core.Infrastructure.AI;

public sealed class AgentFactory(ILoggerFactory loggerFactory) : IAgentFactory
{
    public AIAgent Create(
        AiProvider provider,
        string modelName,
        string apiKey,
        string? instructions = null,
        IList<AITool>? tools = null)
    {
        return provider switch
        {
            AiProvider.Claude => CreateClaudeAgent(modelName, apiKey, instructions, tools),
            AiProvider.OpenAI => CreateOpenAIAgent(modelName, apiKey, instructions, tools),
            _ => throw new NotSupportedException($"Provider '{provider}' não suportado.")
        };
    }

    private AIAgent CreateClaudeAgent(
        string modelName,
        string apiKey,
        string? instructions,
        IList<AITool>? tools)
    {
        var client = new AnthropicClient { ApiKey = apiKey };

        return client.AsAIAgent(
            model: modelName,
            instructions: instructions,
            tools: tools,
            loggerFactory: loggerFactory);
    }

    private AIAgent CreateOpenAIAgent(
        string modelName,
        string apiKey,
        string? instructions,
        IList<AITool>? tools)
    {
        var chatClient = new ChatClient(modelName, apiKey);

        return chatClient.AsAIAgent(
            instructions: instructions,
            tools: tools,
            loggerFactory: loggerFactory);
    }
}
