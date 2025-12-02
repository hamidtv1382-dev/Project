using AnalysisCallUser._01_Domain.Core.Entities;

namespace AnalysisCallUser._03_EndPoint.Models.ViewModels.Admin
{
    public class UserWithRolesViewModel
    {
        public ApplicationUser User { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
    }

   
}
