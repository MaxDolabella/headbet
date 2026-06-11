using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HeadBet.Core.Domain.Entities;

namespace HeadBet.Core.Infrastructure.Data.Configurations;

internal sealed class MatchConfig : IEntityTypeConfiguration<Match>
{
    public void Configure(EntityTypeBuilder<Match> builder)
    {
        builder.ToTable("Match").HasKey(x => x.Id);

        builder.Property(x => x.Id).IsRequired();
        builder.Property(x => x.PoolId).IsRequired();
        builder.Property(x => x.HomeTeamId).IsRequired();
        builder.Property(x => x.AwayTeamId).IsRequired();
        builder.Property(x => x.MatchDate).IsRequired();
        builder.Property(x => x.HomeScore).IsRequired(false);
        builder.Property(x => x.AwayScore).IsRequired(false);
        builder.Property(x => x.Status).IsRequired();
        builder.Property(x => x.Group).HasMaxLength(50).IsRequired(false);
        builder.Property(x => x.Round).IsRequired(false);
        builder.Property(x => x.BroadcastUrl).HasMaxLength(500).IsRequired(false);

        builder.HasOne(x => x.Pool).WithMany();
        builder.HasOne(x => x.HomeTeam).WithMany();
        builder.HasOne(x => x.AwayTeam).WithMany();
    }
}
