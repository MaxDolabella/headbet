using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HeadBet.Core.Domain.Entities;

namespace HeadBet.Core.Infrastructure.Data.Configurations;

internal sealed class TeamConfig : IEntityTypeConfiguration<Team>
{
    public void Configure(EntityTypeBuilder<Team> builder)
    {
        builder.ToTable("Team").HasKey(x => x.Id);

        builder.Property(x => x.Id).IsRequired();
        builder.Property(x => x.PoolId).IsRequired();
        builder.Property(x => x.ExternalId).IsRequired(false);
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Abbreviation).HasMaxLength(10).IsRequired();
        builder.Property(x => x.FlagUrl).HasMaxLength(500).IsRequired(false);

        builder.HasOne(x => x.Pool).WithMany();

        builder.HasIndex(x => new { x.PoolId, x.ExternalId })
            .IsUnique()
            .HasFilter("ExternalId IS NOT NULL");
    }
}
