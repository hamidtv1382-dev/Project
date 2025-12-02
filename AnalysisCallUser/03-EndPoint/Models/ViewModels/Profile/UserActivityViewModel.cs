using AnalysisCallUser._01_Domain.Core.Entities;

namespace AnalysisCallUser._03_EndPoint.Models.ViewModels.Profile
{
    public class UserActivityViewModel
    {
        public int TotalLogins { get; set; }
        public int TotalExports { get; set; }
        public List<UserLoginHistory> RecentLogins { get; set; }
    }
}
