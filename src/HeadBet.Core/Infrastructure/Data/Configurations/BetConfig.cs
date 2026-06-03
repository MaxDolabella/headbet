using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HeadBet.Core.Domain.Entities;

namespace HeadBet.Core.Infrastructure.Data.Configurations;

internal sealed class BetConfig : IEntityTypeConfiguration<Bet>
{
    public void Configure(EntityTypeBuilder<Bet> builder)
    {
        builder.ToTable("Bet").HasKey(x => x.Id);

        builder.Property(x => x.Id).IsRequired();
        builder.Property(x => x.MatchId).IsRequired();
        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.HomeScore).IsRequired();
        builder.Property(x => x.AwayScore).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasOne(x => x.Match).WithMany();
        builder.HasOne(x => x.User).WithMany();

        // Um palpite por usuario por partida
        builder.HasIndex(x => new { x.MatchId, x.UserId }).IsUnique();
    }
}
