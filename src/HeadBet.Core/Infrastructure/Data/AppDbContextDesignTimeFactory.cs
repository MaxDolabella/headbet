using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HeadBet.Core.Infrastructure.Data;

/// <summary>
/// Fábrica usada apenas em design-time pelas EF Core Tools (migrations add / dbcontext scaffold).
/// Em runtime o <see cref="AppDbContext"/> é configurado via DI no host do app (Blazor/MVC).
/// Permite gerar migrations de forma auto-contida com o próprio Core como startup:
///   dotnet ef migrations add &lt;Name&gt; --project src/HeadBet.Core --startup-project src/HeadBet.Core
/// </summary>
public sealed class AppDbContextDesignTimeFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Data Source=headbet.db")
            .Options;

        return new AppDbContext(options);
    }
}
