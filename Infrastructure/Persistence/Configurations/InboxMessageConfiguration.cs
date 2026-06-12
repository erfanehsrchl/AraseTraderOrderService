using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class InboxMessageConfiguration : IEntityTypeConfiguration<InboxMessage>
{
    public void Configure(EntityTypeBuilder<InboxMessage> builder)
    {
        builder.ToTable("InboxMessages");

        builder.HasKey(message => message.Id);

        builder.Property(message => message.MessageId).IsRequired();
        builder.Property(message => message.MessageType).HasMaxLength(250).IsRequired();
        builder.Property(message => message.CreatedAt).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();

        builder.HasIndex(message => message.MessageId).IsUnique();
    }
}
