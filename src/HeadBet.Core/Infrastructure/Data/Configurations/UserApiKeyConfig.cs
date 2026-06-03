using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HeadBet.Core.Domain.Entities;

namespace HeadBet.Core.Infrastructure.Data.Configurations;

internal sealed class UserApiKeyConfig : IEntityTypeConfiguration<UserApiKey>
{
    public void Configure(EntityTypeBuilder<UserApiKey> builder)
    {
        builder.ToTable("UserApiKey").HasKey(x => x.Id);

        builder.Property(x => x.Id).IsRequired();
        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.AiModelId).IsRequired();
        builder.Property(x => x.ApiKey).HasMaxLength(500).IsRequired();
        builder.Property(x => x.IsDefault).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasOne(x => x.User).WithMany();
        builder.HasOne(x => x.AiModel).WithMany();

        builder.HasIndex(x => new { x.UserId, x.AiModelId }).IsUnique();
    }
}
