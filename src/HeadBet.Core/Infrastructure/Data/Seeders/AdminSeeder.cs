using Headsoft.Core.Helpers;
using HeadBet.Core.Domain;
using HeadBet.Core.Domain.Entities;
using HeadBet.Core.Domain.Enums;
using HeadBet.Core.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HeadBet.Core.Infrastructure.Data.Seeders;

public static class AdminSeeder
{
    public static async Task SeedAsync(IServiceProvider services, CancellationToken ct = default)
    {
        await SeedAdminAsync(services, ct);
        await SeedAiModelsAsync(services, ct);
    }

    private static async Task SeedAdminAsync(IServiceProvider services, CancellationToken ct)
    {
        var config = services.GetRequiredService<IConfiguration>();
        var logger = services.GetRequiredService<ILogger<AppDbContext>>();
        var db = services.GetRequiredService<AppDbContext>();
        var hasher = services.GetRequiredService<IPasswordHasher>();

        var email = config["Seeder:Admin:Email"];
        var password = config["Seeder:Admin:Password"];
        var name = config["Seeder:Admin:Name"];

        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(name))
        {
            logger.LogWarning(
                "AdminSeeder: seção 'Admin' (Email/Password/Name) não está completa em appsettings — seed ignorado.");
            return;
        }

        var exists = await db.Set<User>().AnyAsync(u => u.Role == Roles.ADMIN, ct);
        if (exists)
            return;

        var existing = await db.Set<User>().FirstOrDefaultAsync(u => u.Email == email, ct);
        if (existing is not null)
        {
            existing.Role = Roles.ADMIN;
            db.Update(existing);
            await db.SaveChangesAsync(ct);
            logger.LogInformation("AdminSeeder: usuário existente {Email} promovido a Admin.", email);
            return;
        }

        var admin = new User
        {
            Id = UIDGen.NewGuid(),
            Name = name,
            Email = email,
            PasswordHash = hasher.Hash(password),
            Role = Roles.ADMIN
        };

        await db.AddAsync(admin, ct);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("AdminSeeder: admin inicial criado ({Email}).", email);
    }

    private static async Task SeedAiModelsAsync(IServiceProvider services, CancellationToken ct)
    {
        var config = services.GetRequiredService<IConfiguration>();
        var logger = services.GetRequiredService<ILogger<AppDbContext>>();
        var db = services.GetRequiredService<AppDbContext>();

        var entries = config.GetSection("Seeder:AiModels").Get<List<AiModelSeedEntry>>();
        if (entries is null || entries.Count == 0)
        {
            logger.LogWarning("AdminSeeder: seção 'Seeder:AiModels' não encontrada ou vazia — seed ignorado.");
            return;
        }

        var inserted = 0;
        foreach (var entry in entries)
        {
            if (await db.Set<AiModel>().AnyAsync(m => m.Provider == entry.Provider && m.Name == entry.Name, ct))
                continue;

            await db.AddAsync(new AiModel
            {
                Id = UIDGen.NewGuid(),
                Provider = entry.Provider,
                Name = entry.Name,
                DisplayName = entry.DisplayName,
                IsActive = true,
            }, ct);
            inserted++;
        }

        if (inserted > 0)
        {
            await db.SaveChangesAsync(ct);
            logger.LogInformation("AdminSeeder: {Count} AiModel(s) inserido(s).", inserted);
        }
    }

    private sealed class AiModelSeedEntry
    {
        
        public AiProvider Provider { get; set; }
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
    }
}
