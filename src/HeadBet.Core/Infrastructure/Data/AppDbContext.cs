using Microsoft.EntityFrameworkCore;

namespace HeadBet.Core.Infrastructure.Data;

// EF Core migrations (rodar a partir da raiz do repositorio).
// O Core hospeda o pacote EntityFrameworkCore.Design + AppDbContextDesignTimeFactory,
// entao ele mesmo serve de startup (os apps Blazor/MVC nao referenciam o pacote Design):
//   dotnet ef migrations add <Name> --project src/HeadBet.Core --startup-project src/HeadBet.Core
//   dotnet ef database update --project src/HeadBet.Core --startup-project src/HeadBet.Core
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
        ChangeTracker.LazyLoadingEnabled = false;
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTrackingWithIdentityResolution;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
