using AnalysisCallUser._01_Domain.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnalysisCallUser._02_Infrastructure.Data.Configurations
{
    public class OperatorConfiguration : IEntityTypeConfiguration<Operator>
    {
        public void Configure(EntityTypeBuilder<Operator> builder)
        {
            builder.HasKey(e => e.OperatorID);

            builder.Property(e => e.OperatorName).IsRequired().HasMaxLength(100);
        }
    }
}
