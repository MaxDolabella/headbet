using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HeadBet.Core.Domain.Entities;

namespace HeadBet.Core.Infrastructure.Data.Configurations;

internal sealed class ChatMessageConfig : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        builder.ToTable("ChatMessage").HasKey(x => x.Id);

        builder.Property(x => x.Id).IsRequired();
        builder.Property(x => x.PoolId).IsRequired();
        builder.Property(x => x.Scope).IsRequired();
        builder.Property(x => x.MatchId);
        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.Text).IsRequired().HasMaxLength(500);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.IsDeleted).IsRequired();
        builder.Property(x => x.DeletedAt);
        builder.Property(x => x.DeletedByUserId);

        builder.HasOne(x => x.Pool).WithMany();
        builder.HasOne(x => x.Match).WithMany();
        builder.HasOne(x => x.User).WithMany();

        // Carrega o histórico de um contexto (mural ou jogo) em ordem cronológica.
        builder.HasIndex(x => new { x.PoolId, x.Scope, x.MatchId, x.CreatedAt });
    }
}
