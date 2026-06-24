using System.Diagnostics;
using FluentValidation;
using Headsoft.Core;
using Headsoft.Core.Extensions;
using Headsoft.Messaging.Abstractions;
using Headsoft.Messaging.Abstractions.Commands;
using HeadBet.Core.Domain.Interfaces;
using HeadBet.Core.Infrastructure.Data;
using HeadBet.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HeadBet.Core.Application.Commands;

// --- Command ---
// Executa comandos de escrita/DDL (UPDATE/DELETE/INSERT/CREATE/...) dentro de
// uma transação: commit ao final, rollback automático em qualquer exceção.
// A confirmação do usuário acontece na UI ANTES do envio — a transação aqui
// garante atomicidade, não fica aguardando interação (SQLite trava o arquivo
// enquanto há um writer ativo).
public class RunSqlCommand : CommandBase<OperationResult<SqlConsoleResultViewModel>>
{
    public string Sql { get; set; } = string.Empty;
}

// --- Validator ---
public sealed class RunSqlCommandValidator : AbstractValidator<RunSqlCommand>
{
    public RunSqlCommandValidator()
    {
        RuleFor(x => x.Sql)
            .NotEmpty()
                .WithNotification(GenericMessages.FIELD_REQUIRED, "Informe um comando SQL.", nameof(RunSqlCommand.Sql));
    }
}

// --- Handler ---
public sealed class RunSqlCommandHandler(
    AppDbContext db,
    IUserContext userContext,
    ILogger<RunSqlCommandHandler> logger) : ICommandHandler<RunSqlCommand, OperationResult<SqlConsoleResultViewModel>>
{
    public async Task<OperationResult<SqlConsoleResultViewModel>> HandleAsync(RunSqlCommand command, CancellationToken ct)
    {
        if (!userContext.IsAdmin)
            return Result.Warning<SqlConsoleResultViewModel>(GenericMessages.INVALID_OPERATION, "Acesso negado.");

        var bytes = System.Text.Encoding.UTF8.GetByteCount(command.Sql);
        logger.LogWarning("SQL console (escrita) por {User} <{Email}> — {Chars} chars / {Bytes} bytes: {Sql}",
            userContext.Name, userContext.Email, command.Sql.Length, bytes, command.Sql);

        var sw = Stopwatch.StartNew();
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        try
        {
            var affected = await db.Database.ExecuteSqlRawAsync(command.Sql, ct);
            await tx.CommitAsync(ct);
            sw.Stop();

            return Result.Success(new SqlConsoleResultViewModel
            {
                IsQuery = false,
                AffectedRows = affected,
                ElapsedMs = sw.ElapsedMilliseconds,
            });
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync(ct);
            var detail = Flatten(ex);
            // Loga a cadeia COMPLETA de exceções no console do servidor (SQLite/EF
            // costumam esconder o erro real na InnerException).
            logger.LogError(ex, "Erro no SQL console (escrita) — rollback aplicado. Detalhe: {Detail}", detail);
            return Result.Error<SqlConsoleResultViewModel>(GenericMessages.INVALID_OPERATION, detail);
        }
    }

    // Achata Message + todas as InnerException numa única string legível.
    private static string Flatten(Exception ex)
    {
        var sb = new System.Text.StringBuilder();
        for (var e = ex; e is not null; e = e.InnerException)
            sb.Append(e.GetType().Name).Append(": ").AppendLine(e.Message);
        return sb.ToString().TrimEnd();
    }
}
