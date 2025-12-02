using AnalysisCallUser._01_Domain.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnalysisCallUser._02_Infrastructure.Data.Configurations
{
    public class CityConfiguration : IEntityTypeConfiguration<City>
    {
        public void Configure(EntityTypeBuilder<City> builder)
        {
            builder.HasKey(e => e.CityID);

            builder.Property(e => e.CityName).IsRequired().HasMaxLength(100);

            builder.HasOne(e => e.Country)
                .WithMany(c => c.Cities)
                .HasForeignKey(e => e.CountryID)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
