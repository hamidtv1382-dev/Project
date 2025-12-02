using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AnalysisCallUser._03_EndPoint.Filters
{
    public class CustomAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // بررسی اینکه آیا کاربر احراز هویت شده است یا خیر
            if (!context.HttpContext.User.Identity.IsAuthenticated)
            {
                // اگر کاربر لاگین نکرده بود، او را به صفحه لاگین هدایت کن
                context.Result = new RedirectToActionResult("Login", "Account", null);
            }
        }
    }
}
