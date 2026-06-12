using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");

        builder.HasKey(customer => customer.Id);

        builder.Property(customer => customer.NationalCode).HasMaxLength(20).IsRequired();
        builder.Property(customer => customer.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(customer => customer.LastName).HasMaxLength(100).IsRequired();
        builder.Property(customer => customer.FatherName).HasMaxLength(100).IsRequired();
        builder.Property(customer => customer.BirthCertificationNumber).HasMaxLength(50).IsRequired();
        builder.Property(customer => customer.RegistrationNumber).HasMaxLength(50).IsRequired();
        builder.Property(customer => customer.BranchName).HasMaxLength(150).IsRequired();
        builder.Property(customer => customer.MobileNumber).HasMaxLength(20).IsRequired();

        builder.Property(customer => customer.BirthDate).HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(customer => customer.CreatedAt).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();
        builder.Property(customer => customer.UpdatedAt).HasColumnType("timestamp with time zone").IsRequired();

        builder.HasMany(customer => customer.Orders)
            .WithOne(order => order.Customer)
            .HasForeignKey(order => order.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(customer => customer.Wallet)
            .WithOne(wallet => wallet.Customer)
            .HasForeignKey<Wallet>(wallet => wallet.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
