using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HeadBet.Core.Domain.Entities;

namespace HeadBet.Core.Infrastructure.Data.Configurations;

internal sealed class MatchUserScoreConfig : IEntityTypeConfiguration<MatchUserScore>
{
    public void Configure(EntityTypeBuilder<MatchUserScore> builder)
    {
        builder.ToTable("MatchUserScore").HasKey(x => x.Id);

        builder.Property(x => x.Id).IsRequired();
        builder.Property(x => x.MatchId).IsRequired();
        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.Points).IsRequired();
        builder.Property(x => x.AppliedRule).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();

        builder.HasOne(x => x.Match).WithMany().OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.User).WithMany().OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.MatchId, x.UserId }).IsUnique();
    }
}
