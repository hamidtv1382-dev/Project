using AnalysisCallUser._01_Domain.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnalysisCallUser._02_Infrastructure.Data.Configurations
{
    public class CallDetailConfiguration : IEntityTypeConfiguration<CallDetail>
    {
        public void Configure(EntityTypeBuilder<CallDetail> builder)
        {
            builder.HasKey(e => e.DetailID);

            builder.Property(e => e.ANumber).IsRequired().HasMaxLength(20);
            builder.Property(e => e.BNumber).IsRequired().HasMaxLength(20);
            builder.Property(e => e.AccountingTime_SH).HasMaxLength(50);

            builder.HasOne(e => e.OriginCountry)
                .WithMany(c => c.OriginCallDetails)
                .HasForeignKey(e => e.OriginCountryID)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.OriginCity)
                .WithMany(c => c.OriginCallDetails)
                .HasForeignKey(e => e.OriginCityID)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.OriginOperator)
                .WithMany(o => o.OriginCallDetails)
                .HasForeignKey(e => e.OriginOperatorID)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.DestCountry)
                .WithMany(c => c.DestCallDetails)
                .HasForeignKey(e => e.DestCountryID)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.DestCity)
                .WithMany(c => c.DestCallDetails)
                .HasForeignKey(e => e.DestCityID)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.DestOperator)
                .WithMany(o => o.DestCallDetails)
                .HasForeignKey(e => e.DestOperatorID)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.CallType)
                .WithMany(ct => ct.CallDetails)
                .HasForeignKey(e => e.TypeID)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
