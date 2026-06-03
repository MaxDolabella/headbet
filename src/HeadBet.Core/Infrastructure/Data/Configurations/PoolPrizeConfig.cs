using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HeadBet.Core.Domain.Entities;

namespace HeadBet.Core.Infrastructure.Data.Configurations;

internal sealed class PoolPrizeConfig : IEntityTypeConfiguration<PoolPrize>
{
    public void Configure(EntityTypeBuilder<PoolPrize> builder)
    {
        builder.ToTable("PoolPrize").HasKey(x => new { x.PoolId, x.Position });

        builder.Property(x => x.PoolId).IsRequired();
        builder.Property(x => x.Position).IsRequired();
        builder.Property(x => x.Percentage).IsRequired(false);
        builder.Property(x => x.FixedAmount).IsRequired(false);

        builder.HasOne(x => x.Pool).WithMany();
    }
}
