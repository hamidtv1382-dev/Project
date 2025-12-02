using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AnalysisCallUser._03_EndPoint.Filters
{
    public class RoleBasedAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _roles;

        public RoleBasedAuthorizeAttribute(params string[] roles)
        {
            _roles = roles;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (!context.HttpContext.User.Identity.IsAuthenticated)
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            var userRoles = context.HttpContext.User.Claims.Where(c => c.Type == System.Security.Claims.ClaimTypes.Role).Select(c => c.Value);

            if (!_roles.Any(role => userRoles.Contains(role)))
            {
                // اگر کاربر نقش مورد نظر را نداشت، به صفحه دسترسی غیرمجاز هدایت شود
                context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
            }
        }
    }
}
