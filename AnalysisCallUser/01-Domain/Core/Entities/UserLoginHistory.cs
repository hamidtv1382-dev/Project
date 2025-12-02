namespace AnalysisCallUser._01_Domain.Core.Entities
{
    public class UserLoginHistory
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime LoginTime { get; set; }
        public DateTime? LogoutTime { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public bool IsSuccessful { get; set; }
        public string FailureReason { get; set; }

        public ApplicationUser User { get; set; }
    }
}
