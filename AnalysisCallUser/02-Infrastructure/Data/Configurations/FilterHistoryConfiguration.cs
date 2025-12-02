using AnalysisCallUser._01_Domain.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnalysisCallUser._02_Infrastructure.Data.Configurations
{
    public class FilterHistoryConfiguration : IEntityTypeConfiguration<FilterHistory>
    {
        public void Configure(EntityTypeBuilder<FilterHistory> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.FilterName).IsRequired().HasMaxLength(100);
            builder.Property(e => e.FilterParameters).IsRequired();

            builder.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            builder.HasOne(e => e.User)
                .WithMany(u => u.FilterHistories)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
