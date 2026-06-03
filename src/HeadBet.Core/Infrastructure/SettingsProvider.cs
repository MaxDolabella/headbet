using System.Text.Json;
using HeadBet.Core.Domain.Interfaces;
using HeadBet.Core.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HeadBet.Core.Infrastructure;

public sealed class SettingsProvider(AppDbContext context) : ISettingsProvider
{
    public async Task<string?> GetJsonAsync(string name, CancellationToken ct = default)
    {
        var setting = await context.Set<Domain.Entities.AppSetting>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Name == name, ct);

        return setting?.Data;
    }

    public async Task<T?> GetAsync<T>(string name, CancellationToken ct = default) where T : class
    {
        var json = await GetJsonAsync(name, ct);

        if (json is null)
            return null;

        return JsonSerializer.Deserialize<T>(json);
    }
}
