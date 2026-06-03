using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HeadBet.Core.Domain.Entities;

namespace HeadBet.Core.Infrastructure.Data.Configurations;

internal sealed class PoolConfig : IEntityTypeConfiguration<Pool>
{
    public void Configure(EntityTypeBuilder<Pool> builder)
    {
        builder.ToTable("Pool").HasKey(x => x.Id);

        builder.Property(x => x.Id).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500).IsRequired();
        builder.Property(x => x.IsPaid).IsRequired();
        builder.Property(x => x.EntryFee).IsRequired(false);
        builder.Property(x => x.CollectedAmount).IsRequired(false);
        builder.Property(x => x.AutoAccept).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.IsPublic).IsRequired();
        builder.Property(x => x.PrizeMode).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.InviteCode).HasMaxLength(32).IsRequired();

        builder.HasIndex(x => x.InviteCode).IsUnique();
    }
}
