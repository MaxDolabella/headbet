using HeadBet.Core.Infrastructure.Data;
using HeadBet.Core.Infrastructure.Data.Seeders;
using Microsoft.EntityFrameworkCore;

namespace HeadBet.Blazor.Infrastructure.Data;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(this WebApplication app, CancellationToken ct = default)
    {
        using var scope = app.Services.CreateScope();
        var sp = scope.ServiceProvider;

        var db = sp.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync(ct);

        await AdminSeeder.SeedAsync(sp, ct);
    }
}
