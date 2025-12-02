using Microsoft.AspNetCore.Identity;
namespace AnalysisCallUser._01_Domain.Core.Entities
{
    public class ApplicationRole : IdentityRole<int>
    {
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }

}
