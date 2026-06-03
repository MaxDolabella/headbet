using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using HeadBet.Core.Domain.Enums;

namespace HeadBet.Core.Domain.Interfaces;

public interface IAgentFactory
{
    AIAgent Create(
        AiProvider provider,
        string modelName,
        string apiKey,
        string? instructions = null,
        IList<AITool>? tools = null);
}
