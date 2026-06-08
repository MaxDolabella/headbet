using FluentValidation;
using Headsoft.Core;
using Headsoft.Core.Extensions;
using Headsoft.Core.Helpers;
using Headsoft.Core.Interfaces.Data;
using Headsoft.Messaging.Abstractions;
using Headsoft.Messaging.Abstractions.Commands;
using HeadBet.Core.Domain.Enums;
using HeadBet.Core.Domain.Interfaces;
using HeadBet.Core.Domain.Interfaces.Repositories;

namespace HeadBet.Core.Application.Commands;

// --- Command ---
public class DeleteChatMessageCommand : CommandBase<OperationResult>
{
    public Guid MessageId { get; set; }

    public DeleteChatMessageCommand() { }

    public DeleteChatMessageCommand(Guid messageId) => MessageId = messageId;
}

// --- Validator ---
public sealed class DeleteChatMessageCommandValidator : AbstractValidator<DeleteChatMessageCommand>
{
    public DeleteChatMessageCommandValidator()
    {
        RuleFor(x => x.MessageId)
            .NotEmpty()
                .WithNotification(GenericMessages.FIELD_REQUIRED, "Mensagem é obrigatória.", nameof(DeleteChatMessageCommand.MessageId));
    }
}

// --- Handler ---
public sealed class DeleteChatMessageCommandHandler(
    IChatMessageRepository chatRepository,
    IPoolMemberRepository memberRepository,
    IUnitOfWork uow,
    IUserContext userContext,
    IChatBroadcaster broadcaster) : ICommandHandler<DeleteChatMessageCommand, OperationResult>
{
    public async Task<OperationResult> HandleAsync(DeleteChatMessageCommand command, CancellationToken ct)
    {
        var userId = userContext.UserId;

        var message = await chatRepository.GetAsync(m => m.Id == command.MessageId, @readonly: false, ct);
        if (message is null || message.IsDeleted)
            return Result.Warning(GenericMessages.INVALID_OPERATION, "Mensagem não encontrada.");

        // Pode apagar: admin do app, ou admin do bolão da mensagem.
        var canModerate = userContext.IsAdmin
                          || await memberRepository.AnyAsync(
                              m => m.PoolId == message.PoolId
                                   && m.UserId == userId
                                   && m.Role == PoolMemberRole.Admin
                                   && m.Status == PoolMemberStatus.Active,
                              ct);

        if (!canModerate)
            return Result.Warning(GenericMessages.INVALID_OPERATION, "Apenas administradores podem apagar mensagens.");

        message.IsDeleted = true;
        message.DeletedAt = DateTime.UtcNow;
        message.DeletedByUserId = userId;

        var saveResult = await uow.SaveChangesAsync(ct);
        if (!saveResult.IsValid)
            return saveResult;

        var contextKey = ChatContextKeys.For(message.Scope, message.PoolId, message.MatchId);
        await broadcaster.PublishAsync(new ChatEvent(contextKey, null, message.Id));

        return saveResult;
    }
}
