using AutoMapper;
using Headsoft.Core;
using Headsoft.Core.Helpers;
using Headsoft.Core.Interfaces.Data;
using Headsoft.Messaging.Abstractions;
using Headsoft.Messaging.Abstractions.Commands;
using HeadBet.Core.Domain.Entities;
using HeadBet.Core.Domain.Enums;
using HeadBet.Core.Domain.Interfaces.Repositories;

namespace HeadBet.Core.Application.Commands;

// --- Command ---
public class CreateAiModelCommand : CommandBase<OperationResult<Guid>>
{
    public Guid Id { get; set; } = UIDGen.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public AiProvider Provider { get; set; }
    public bool IsActive { get; set; } = true;
}

// --- Handler ---
public sealed class CreateAiModelCommandHandler(
    IAiModelRepository repository,
    IUnitOfWork uow,
    IMapper mapper) : ICommandHandler<CreateAiModelCommand, OperationResult<Guid>>
{
    public async Task<OperationResult<Guid>> HandleAsync(CreateAiModelCommand command, CancellationToken ct)
    {
        var entity = mapper.Map<AiModel>(command);

        await repository.AddAsync(entity, ct);
        var result = await uow.SaveChangesAsync(ct);

        return result.IsValid
            ? Result.Success(entity.Id)
            : result.Cast<Guid>();
    }
}
