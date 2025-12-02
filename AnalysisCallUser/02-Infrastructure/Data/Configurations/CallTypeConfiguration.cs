using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AnalysisCallUser._01_Domain.Core.Entities;


namespace AnalysisCallUser._02_Infrastructure.Data.Configurations
{
    public class CallTypeConfiguration : IEntityTypeConfiguration<CallType>
    {
        public void Configure(EntityTypeBuilder<CallType> builder)
        {
            builder.HasKey(e => e.TypeID);

            builder.Property(e => e.TypeName).IsRequired().HasMaxLength(50);
        }
    }
}
