using AnalysisCallUser._01_Domain.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnalysisCallUser._02_Infrastructure.Data.Configurations
{
    public class UserLoginHistoryConfiguration : IEntityTypeConfiguration<UserLoginHistory>
    {
        public void Configure(EntityTypeBuilder<UserLoginHistory> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.IpAddress).HasMaxLength(45);
            builder.Property(e => e.UserAgent).HasMaxLength(500);
            builder.Property(e => e.FailureReason).HasMaxLength(255);

            builder.Property(e => e.LoginTime).HasDefaultValueSql("GETUTCDATE()");

            builder.HasOne(e => e.User)
                .WithMany(u => u.LoginHistories)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
