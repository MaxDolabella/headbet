using AutoMapper;
using Headsoft.Core;
using Headsoft.Core.Interfaces.Data;
using Headsoft.Messaging.Abstractions;
using Headsoft.Messaging.Abstractions.Commands;
using HeadBet.Core.Domain.Entities;
using HeadBet.Core.Domain.Enums;
using HeadBet.Core.Domain.Interfaces.Repositories;

namespace HeadBet.Core.Application.Commands;

// --- Command ---
public class UpdateAiModelCommand : CommandBase<OperationResult>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public AiProvider Provider { get; set; }
    public bool IsActive { get; set; }
}

// --- Handler ---
public sealed class UpdateAiModelCommandHandler(
    IAiModelRepository repository,
    IUnitOfWork uow,
    IMapper mapper) : ICommandHandler<UpdateAiModelCommand, OperationResult>
{
    public async Task<OperationResult> HandleAsync(UpdateAiModelCommand command, CancellationToken ct)
    {
        var entity = await repository.GetAsync(e => e.Id == command.Id, @readonly: false, ct);
        if (entity is null)
            return Result.Warning(GenericMessages.ITEM_NOT_FOUND, "Modelo de IA não encontrado.");

        mapper.Map(command, entity);
        var result = await uow.SaveChangesAsync(ct);

        return result;
    }
}
