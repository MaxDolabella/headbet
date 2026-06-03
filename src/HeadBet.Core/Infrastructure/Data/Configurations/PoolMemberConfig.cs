using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HeadBet.Core.Domain.Entities;

namespace HeadBet.Core.Infrastructure.Data.Configurations;

internal sealed class PoolMemberConfig : IEntityTypeConfiguration<PoolMember>
{
    public void Configure(EntityTypeBuilder<PoolMember> builder)
    {
        builder.ToTable("PoolMember").HasKey(x => new { x.PoolId, x.UserId });

        builder.Property(x => x.PoolId).IsRequired();
        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.Role).IsRequired();
        builder.Property(x => x.Status).IsRequired();
        builder.Property(x => x.JoinedAt).IsRequired();

        builder.HasOne(x => x.Pool).WithMany();
        builder.HasOne(x => x.User).WithMany();
    }
}
