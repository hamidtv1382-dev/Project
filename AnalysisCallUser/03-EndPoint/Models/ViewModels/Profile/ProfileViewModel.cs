using AnalysisCallUser._01_Domain.Core.Enums;

namespace AnalysisCallUser._03_EndPoint.Models.ViewModels.Profile
{
    public class ProfileViewModel
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public UserRole Role { get; set; }
    }
}
