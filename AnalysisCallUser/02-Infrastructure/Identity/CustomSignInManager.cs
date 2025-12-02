using AnalysisCallUser._01_Domain.Core.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace AnalysisCallUser._02_Infrastructure.Identity
{
    public class CustomSignInManager : SignInManager<ApplicationUser>
    {
        public CustomSignInManager(
            UserManager<ApplicationUser> userManager,
            IHttpContextAccessor contextAccessor,
            IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory,
            IOptions<IdentityOptions> optionsAccessor,
            ILogger<SignInManager<ApplicationUser>> logger,
            IAuthenticationSchemeProvider schemes,
            IUserConfirmation<ApplicationUser> confirmation)
            : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
        {
        }

        // مثال: بازنویسی متد PasswordSignInAsync برای ثبت لاگین‌های ناموفق
        public override async Task<SignInResult> PasswordSignInAsync(string userName, string password, bool isPersistent, bool lockoutOnFailure)
        {
            var result = await base.PasswordSignInAsync(userName, password, isPersistent, lockoutOnFailure);

            // اگر لاگین ناموفق بود، می‌توانید یک رویداد لاگ کنید یا یک سرویس نوتیفیکیشن را فراخوانی کنید
            if (!result.Succeeded)
            {
                // مثال: logger.LogWarning($"Failed login attempt for user: {userName}");
            }

            return result;
        }
    }
}
