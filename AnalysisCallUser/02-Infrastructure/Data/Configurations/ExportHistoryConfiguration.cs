using AnalysisCallUser._01_Domain.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnalysisCallUser._02_Infrastructure.Data.Configurations
{
    public class ExportHistoryConfiguration : IEntityTypeConfiguration<ExportHistory>
    {
        public void Configure(EntityTypeBuilder<ExportHistory> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.ExportType).IsRequired().HasMaxLength(20);
            builder.Property(e => e.FilePath).HasMaxLength(500);
            builder.Property(e => e.ErrorMessage).HasMaxLength(500);

            builder.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            builder.HasOne(e => e.User)
                .WithMany(u => u.ExportHistories)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
