using System.Data;
using System.Diagnostics;
using Headsoft.Messaging.Abstractions.Queries;
using HeadBet.Core.Domain.Interfaces;
using HeadBet.Core.Infrastructure.Data;
using HeadBet.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HeadBet.Core.Application.Queries;

// --- Query ---
public record RunSqlQuery(string Sql) : QueryBase<SqlConsoleResultViewModel>;

// --- Handler ---
// Executa SELECT/PRAGMA via ADO.NET cru (ExecuteReader) para suportar colunas
// arbitrárias — o EF não mapeia bem result sets dinâmicos. Acesso restrito ao
// admin global; o handler revalida além do [Authorize] da página.
public sealed class RunSqlQueryHandler(
    AppDbContext db,
    IUserContext userContext,
    ILogger<RunSqlQueryHandler> logger) : QueryHandlerBase<RunSqlQuery, SqlConsoleResultViewModel>
{
    private const int MAX_ROWS = 1000;

    public override async Task<SqlConsoleResultViewModel> HandleAsync(RunSqlQuery query, CancellationToken ct)
    {
        var vm = new SqlConsoleResultViewModel { IsQuery = true };

        if (!userContext.IsAdmin)
        {
            vm.Error = "Acesso negado.";
            return vm;
        }

        if (string.IsNullOrWhiteSpace(query.Sql))
        {
            vm.Error = "Informe um comando SQL.";
            return vm;
        }

        logger.LogWarning("SQL console (consulta) por {User} <{Email}>: {Sql}",
            userContext.Name, userContext.Email, query.Sql);

        var connection = db.Database.GetDbConnection();
        var openedHere = connection.State != ConnectionState.Open;
        var sw = Stopwatch.StartNew();
        try
        {
            if (openedHere)
                await connection.OpenAsync(ct);

            await using var cmd = connection.CreateCommand();
            cmd.CommandText = query.Sql;

            await using var reader = await cmd.ExecuteReaderAsync(ct);

            for (var i = 0; i < reader.FieldCount; i++)
                vm.Columns.Add(reader.GetName(i));

            while (await reader.ReadAsync(ct))
            {
                if (vm.Rows.Count >= MAX_ROWS)
                {
                    vm.Truncated = true;
                    break;
                }

                var row = new object?[reader.FieldCount];
                for (var i = 0; i < reader.FieldCount; i++)
                {
                    var value = reader.GetValue(i);
                    row[i] = value is DBNull ? null : value;
                }
                vm.Rows.Add(row);
            }
        }
        catch (Exception ex)
        {
            vm.Error = ex.Message;
            logger.LogError(ex, "Erro no SQL console (consulta).");
        }
        finally
        {
            sw.Stop();
            vm.ElapsedMs = sw.ElapsedMilliseconds;
            if (openedHere && connection.State == ConnectionState.Open)
                await connection.CloseAsync();
        }

        return vm;
    }
}
