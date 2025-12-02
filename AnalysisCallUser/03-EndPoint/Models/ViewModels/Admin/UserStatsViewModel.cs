namespace AnalysisCallUser._03_EndPoint.Models.ViewModels.Admin
{
    public class UserStatsViewModel
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int InactiveUsers { get; set; }
        public int NewUsersThisMonth { get; set; }
    }
}
