using FluentValidation;
using Headsoft.Core;
using Headsoft.Core.Extensions;
using Headsoft.Core.Helpers;
using Headsoft.Core.Interfaces.Data;
using Headsoft.Messaging.Abstractions;
using Headsoft.Messaging.Abstractions.Commands;
using HeadBet.Core.Domain.Entities;
using HeadBet.Core.Domain.Enums;
using HeadBet.Core.Domain.Interfaces;
using HeadBet.Core.Domain.Interfaces.Repositories;
using HeadBet.Core.Extensions;
using HeadBet.Core.Models;

namespace HeadBet.Core.Application.Commands;

// --- Command ---
public class PostChatMessageCommand : CommandBase<OperationResult>
{
    public Guid PoolId { get; set; }
    public ChatScope Scope { get; set; }
    public Guid? MatchId { get; set; }
    public string Text { get; set; } = string.Empty;
}

// --- Validator ---
public sealed class PostChatMessageCommandValidator : AbstractValidator<PostChatMessageCommand>
{
    public const int MAX_LENGTH = 500;

    public PostChatMessageCommandValidator()
    {
        RuleFor(x => x.PoolId)
            .NotEmpty()
                .WithNotification(GenericMessages.FIELD_REQUIRED, "Bolão é obrigatório.", nameof(PostChatMessageCommand.PoolId));

        RuleFor(x => x.Text)
            .NotEmpty()
                .WithNotification(GenericMessages.FIELD_REQUIRED, "A mensagem não pode ser vazia.", nameof(PostChatMessageCommand.Text))
            .MaximumLength(MAX_LENGTH)
                .WithNotification(GenericMessages.FIELD_INVALID, $"A mensagem deve ter no máximo {MAX_LENGTH} caracteres.", nameof(PostChatMessageCommand.Text));

        RuleFor(x => x.MatchId)
            .NotEmpty()
                .WithNotification(GenericMessages.FIELD_REQUIRED, "Jogo é obrigatório para comentário de jogo.", nameof(PostChatMessageCommand.MatchId))
            .When(x => x.Scope == ChatScope.Match);
    }
}

// --- Handler ---
public sealed class PostChatMessageCommandHandler(
    IPoolMemberRepository memberRepository,
    IChatMessageRepository chatRepository,
    IUnitOfWork uow,
    IUserContext userContext,
    IChatBroadcaster broadcaster) : ICommandHandler<PostChatMessageCommand, OperationResult>
{
    public async Task<OperationResult> HandleAsync(PostChatMessageCommand command, CancellationToken ct)
    {
        var userId = userContext.UserId;

        var isMember = await memberRepository.AnyAsync(
            m => m.PoolId == command.PoolId
                 && m.UserId == userId
                 && m.Status == PoolMemberStatus.Active,
            ct);

        if (!isMember)
            return Result.Warning(GenericMessages.INVALID_OPERATION, "Apenas membros ativos do bolão podem comentar.");

        var text = command.Text.Trim();
        if (string.IsNullOrEmpty(text))
            return Result.Warning(GenericMessages.INVALID_OPERATION, "A mensagem não pode ser vazia.");

        var matchId = command.Scope == ChatScope.Match ? command.MatchId : null;
        var createdAt = DateTime.UtcNow;

        var message = new ChatMessage
        {
            Id = UIDGen.NewGuid(),
            PoolId = command.PoolId,
            Scope = command.Scope,
            MatchId = matchId,
            UserId = userId,
            Text = text,
            CreatedAt = createdAt,
            IsDeleted = false,
        };

        await chatRepository.AddAsync(message, ct);

        var saveResult = await uow.SaveChangesAsync(ct);
        if (!saveResult.IsValid)
            return saveResult;

        var view = new ChatMessageViewModel
        {
            Id = message.Id,
            UserId = userId,
            UserName = userContext.Name,
            Text = text,
            CreatedAt = createdAt.ToBrt(),
        };

        var contextKey = ChatContextKeys.For(command.Scope, command.PoolId, matchId);
        await broadcaster.PublishAsync(new ChatEvent(contextKey, view, null));

        return saveResult;
    }
}
