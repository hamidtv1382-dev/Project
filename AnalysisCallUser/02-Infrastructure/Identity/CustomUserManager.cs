using AnalysisCallUser._01_Domain.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace AnalysisCallUser._02_Infrastructure.Identity
{
    public class CustomUserManager : UserManager<ApplicationUser>
    {
        public CustomUserManager(
            IUserStore<ApplicationUser> store,
            IOptions<IdentityOptions> optionsAccessor,
            IPasswordHasher<ApplicationUser> passwordHasher,
            IEnumerable<IUserValidator<ApplicationUser>> userValidators,
            IEnumerable<IPasswordValidator<ApplicationUser>> passwordValidators,
            ILookupNormalizer keyNormalizer,
            IdentityErrorDescriber errors,
            IServiceProvider services,
            ILogger<UserManager<ApplicationUser>> logger)
            : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {
        }

        // مثال: متد کمکی برای پیدا کردن کاربر به همراه اطلاعات لاگین اخیرش
        public async Task<ApplicationUser> FindUserWithLastLoginAsync(int userId)
        {
            var user = await FindByIdAsync(userId.ToString());
            // در اینجا می‌توان منطق‌های اضافی برای لود کردن اطلاعات مرتبط را اضافه کرد
            // مثلاً اگر یک ریپازیتوری برای UserLoginHistory داشتید، آن را اینجا فراخوانی می‌کردید
            return user;
        }
    }
}
