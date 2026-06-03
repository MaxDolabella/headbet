using FluentValidation;
using Headsoft.Core;
using Headsoft.Core.Extensions;
using Headsoft.Core.Interfaces.Data;
using Headsoft.Messaging.Abstractions;
using Headsoft.Messaging.Abstractions.Commands;
using HeadBet.Core.Domain.Interfaces;
using HeadBet.Core.Domain.Interfaces.Repositories;

namespace HeadBet.Core.Application.Commands;

// --- Command ---
/// <summary>
/// Atualização self-service do próprio perfil. O usuário-alvo é sempre o
/// autenticado (resolvido via IUserContext no handler) — nunca um Id vindo do cliente.
/// </summary>
public class UpdateProfileCommand : CommandBase<OperationResult>
{
    public string Name { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public bool WhatsAppOptIn { get; set; }
}

// --- Validator ---
public sealed class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
                .WithNotification(GenericMessages.FIELD_REQUIRED, "Nome é obrigatório.", nameof(UpdateProfileCommand.Name))
            .MaximumLength(100)
                .WithNotification(GenericMessages.FIELD_LENGTH, "Nome deve ter no máximo 100 caracteres.", nameof(UpdateProfileCommand.Name));

        When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber), () =>
        {
            RuleFor(x => x.PhoneNumber!)
                .MaximumLength(20)
                    .WithNotification(GenericMessages.FIELD_LENGTH, "Telefone deve ter no máximo 20 caracteres.", nameof(UpdateProfileCommand.PhoneNumber))
                .Matches(@"^\+?[0-9\s()\-]{8,20}$")
                    .WithNotification(GenericMessages.FIELD_FORMAT, "Telefone em formato inválido.", nameof(UpdateProfileCommand.PhoneNumber));
        });

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
                .WithNotification(GenericMessages.FIELD_REQUIRED, "Informe um telefone para receber notificações no WhatsApp.", nameof(UpdateProfileCommand.PhoneNumber))
            .When(x => x.WhatsAppOptIn);
    }
}

// --- Handler ---
public sealed class UpdateProfileCommandHandler(
    IUserRepository repository,
    IUnitOfWork uow,
    IUserContext userContext) : ICommandHandler<UpdateProfileCommand, OperationResult>
{
    public async Task<OperationResult> HandleAsync(UpdateProfileCommand command, CancellationToken ct)
    {
        var userId = userContext.RequireUserId();

        var entity = await repository.GetAsync(e => e.Id == userId, @readonly: false, ct);
        if (entity is null)
            return Result.Warning(GenericMessages.ITEM_NOT_FOUND, "Usuário não encontrado.");

        entity.Name = command.Name;
        entity.PhoneNumber = string.IsNullOrWhiteSpace(command.PhoneNumber) ? null : command.PhoneNumber.Trim();
        entity.WhatsAppOptIn = command.WhatsAppOptIn;

        return await uow.SaveChangesAsync(ct);
    }
}
