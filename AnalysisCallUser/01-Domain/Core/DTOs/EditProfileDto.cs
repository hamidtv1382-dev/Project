using System.ComponentModel.DataAnnotations;

namespace AnalysisCallUser._01_Domain.Core.DTOs
{
    public partial class OperatorPerformanceDto
    {
        public class EditProfileDto
        {
            public int Id { get; set; }

            [Required]
            [StringLength(50)]
            public string FirstName { get; set; }

            [Required]
            [StringLength(50)]
            public string LastName { get; set; }

            [Phone]
            public string PhoneNumber { get; set; }
        }
    }
}
