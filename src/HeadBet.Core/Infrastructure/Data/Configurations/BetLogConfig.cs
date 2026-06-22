using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HeadBet.Core.Domain.Entities;

namespace HeadBet.Core.Infrastructure.Data.Configurations;

internal sealed class BetLogConfig : IEntityTypeConfiguration<BetLog>
{
    public void Configure(EntityTypeBuilder<BetLog> builder)
    {
        builder.ToTable("BetLog").HasKey(x => x.Id);

        builder.Property(x => x.Id).IsRequired();
        builder.Property(x => x.MatchId).IsRequired();
        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.PoolId).IsRequired();
        builder.Property(x => x.Action).IsRequired();
        builder.Property(x => x.OldHomeScore).IsRequired(false);
        builder.Property(x => x.OldAwayScore).IsRequired(false);
        builder.Property(x => x.NewHomeScore).IsRequired();
        builder.Property(x => x.NewAwayScore).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();

        // Auditoria append-only: índices não-únicos, apenas para consulta.
        builder.HasIndex(x => x.PoolId);
        builder.HasIndex(x => new { x.MatchId, x.UserId });
    }
}
