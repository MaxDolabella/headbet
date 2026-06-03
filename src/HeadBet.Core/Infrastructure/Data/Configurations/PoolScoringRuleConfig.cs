using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HeadBet.Core.Domain.Entities;

namespace HeadBet.Core.Infrastructure.Data.Configurations;

internal sealed class PoolScoringRuleConfig : IEntityTypeConfiguration<PoolScoringRule>
{
    public void Configure(EntityTypeBuilder<PoolScoringRule> builder)
    {
        builder.ToTable("PoolScoringRule").HasKey(x => new { x.PoolId, x.ScoreType });

        builder.Property(x => x.PoolId).IsRequired();
        builder.Property(x => x.ScoreType).IsRequired();
        builder.Property(x => x.Points).IsRequired();

        builder.HasOne(x => x.Pool).WithMany();
    }
}
