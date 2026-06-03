using FluentValidation;
using Headsoft.Core;
using Headsoft.Core.Extensions;
using Headsoft.Messaging.Abstractions;
using Headsoft.Messaging.Abstractions.Commands;
using HeadBet.Core.Application.DTOs;
using HeadBet.Core.Domain.Interfaces;
using HeadBet.Core.Domain.Interfaces.Repositories;

namespace HeadBet.Core.Application.Commands;

// --- Command ---
public class LoginUserCommand : CommandBase<OperationResult<UserSessionDTO>>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

// --- Validator ---
public sealed class LoginUserCommandValidator : AbstractValidator<LoginUserCommand>
{
    public LoginUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
                .WithNotification(GenericMessages.FIELD_REQUIRED, "E-mail é obrigatório.", nameof(LoginUserCommand.Email))
            .EmailAddress()
                .WithNotification(GenericMessages.FIELD_FORMAT, "E-mail em formato inválido.", nameof(LoginUserCommand.Email));

        RuleFor(x => x.Password)
            .NotEmpty()
                .WithNotification(GenericMessages.FIELD_REQUIRED, "Senha é obrigatória.", nameof(LoginUserCommand.Password));
    }
}

// --- Handler ---
public sealed class LoginUserCommandHandler(
    IUserRepository repository,
    IPasswordHasher hasher) : ICommandHandler<LoginUserCommand, OperationResult<UserSessionDTO>>
{
    private const string INVALID_CREDENTIALS_DETAILS = "E-mail ou senha inválidos.";

    public async Task<OperationResult<UserSessionDTO>> HandleAsync(LoginUserCommand command, CancellationToken ct)
    {
        var user = await repository.GetByEmailAsync(command.Email, @readonly: true, ct);
        if (user is null)
            return Result.Error<UserSessionDTO>(GenericMessages.UNAUTHORIZED, INVALID_CREDENTIALS_DETAILS);

        if (string.IsNullOrEmpty(user.PasswordHash) || !hasher.Verify(command.Password, user.PasswordHash))
            return Result.Error<UserSessionDTO>(GenericMessages.UNAUTHORIZED, INVALID_CREDENTIALS_DETAILS);

        return Result.Success(new UserSessionDTO(user.Id, user.Name, user.Email, user.Role));
    }
}
