using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using System.Text;

namespace AnalysisCallUser._03_EndPoint.Filters
{

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class CacheAttribute : ActionFilterAttribute
    {
        private readonly int _duration;
        private readonly IMemoryCache _cache;

        public CacheAttribute(int duration, IMemoryCache cache)
        {
            _duration = duration;
            _cache = cache;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var cacheKey = GenerateCacheKeyFromRequest(context.HttpContext.Request);
            if (!_cache.TryGetValue(cacheKey, out _))
            {
                return; // اگر در کش نبود، اجازه اجرای اکشن را بده
            }

            var cachedResult = _cache.Get(cacheKey) as IActionResult;
            context.Result = cachedResult; // اگر در کش بود، نتیجه کش شده را برگردان
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            if (!context.ExceptionHandled && context.Result is IActionResult result)
            {
                var cacheKey = GenerateCacheKeyFromRequest(context.HttpContext.Request);
                _cache.Set(cacheKey, result, TimeSpan.FromSeconds(_duration));
            }
        }

        private string GenerateCacheKeyFromRequest(HttpRequest request)
        {
            var keyBuilder = new StringBuilder();
            keyBuilder.Append($"{request.Path}_{request.QueryString}");

            // اگر کاربر لاگین کرده باشد، اطلاعات او را نیز در کلید کش لحاظ کن
            if (request.HttpContext.User.Identity.IsAuthenticated)
            {
                keyBuilder.Append($"_User:{request.HttpContext.User.Identity.Name}");
            }

            return keyBuilder.ToString();
        }
    }
}
