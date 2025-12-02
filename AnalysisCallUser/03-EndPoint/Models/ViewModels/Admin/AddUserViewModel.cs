using System.ComponentModel.DataAnnotations;

namespace AnalysisCallUser._03_EndPoint.Models.ViewModels.Admin
{
    public class AddUserViewModel
    {
        [Required(ErrorMessage = "ایمیل الزامی است")]
        [EmailAddress(ErrorMessage = "ایمیل معتبر نیست")]
        [Display(Name = "ایمیل")]
        public string Email { get; set; }

        [Required(ErrorMessage = "رمز عبور الزامی است")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "رمز عبور باید حداقل 8 کاراکتر باشد")]
        [DataType(DataType.Password)]
        [Display(Name = "رمز عبور")]
        public string Password { get; set; }

        [Required(ErrorMessage = "تایید رمز عبور الزامی است")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "رمز عبور و تایید آن باید یکسان باشند")]
        [Display(Name = "تایید رمز عبور")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "نام الزامی است")]
        [Display(Name = "نام")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "نام خانوادگی الزامی است")]
        [Display(Name = "نام خانوادگی")]
        public string LastName { get; set; }

        [Phone(ErrorMessage = "شماره تلفن معتبر نیست")]
        [Display(Name = "شماره تلفن")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "نقش الزامی است")]
        [Display(Name = "نقش")]
        public string Role { get; set; }
    }
}
