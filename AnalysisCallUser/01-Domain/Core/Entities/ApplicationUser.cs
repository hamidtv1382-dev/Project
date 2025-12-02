using AnalysisCallUser._01_Domain.Core.Enums;
using Microsoft.AspNetCore.Identity;

namespace AnalysisCallUser._01_Domain.Core.Entities
{
    public class ApplicationUser : IdentityUser<int>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public string? ProfilePicture { get; set; }
        public bool IsActive { get; set; }
        public ThemeMode ThemePreference { get; set; }

        public ICollection<UserLoginHistory> LoginHistories { get; set; }
        public ICollection<FilterHistory> FilterHistories { get; set; }
        public ICollection<ExportHistory> ExportHistories { get; set; }

    }
}
