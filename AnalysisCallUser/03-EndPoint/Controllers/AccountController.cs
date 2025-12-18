// AnalysisCallUser._03_EndPoint.Controllers.AccountController.cs
using AnalysisCallUser._01_Domain.Core.Contracts;
using AnalysisCallUser._01_Domain.Core.Entities;
using AnalysisCallUser._01_Domain.Services;
using AnalysisCallUser._03_EndPoint.Models.ViewModels.Account;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Session;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AnalysisCallUser._03_EndPoint.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IUserService _userService;
        private readonly IValidator<LoginViewModel> _loginValidator;
        private readonly IValidator<RegisterViewModel> _registerValidator;
        private readonly ICaptchaService _captchaService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            SignInManager<ApplicationUser> signInManager,
            IUserService userService,
            IValidator<LoginViewModel> loginValidator,
            IValidator<RegisterViewModel> registerValidator,
            ICaptchaService captchaService,
            ILogger<AccountController> logger)
        {
            _signInManager = signInManager;
            _userService = userService;
            _loginValidator = loginValidator;
            _registerValidator = registerValidator;
            _captchaService = captchaService;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            // تولید کپچای جدید برای نمایش
            var captchaCode = _captchaService.GenerateCaptchaCode();
            HttpContext.Session.SetString("CaptchaCode", captchaCode);

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            var validationResult = await _loginValidator.ValidateAsync(model);
            if (!validationResult.IsValid)
            {
                ModelState.AddModelError("", validationResult.Errors.FirstOrDefault().ErrorMessage);
                // تولید کپچای جدید در صورت خطا
                var captchaCode = _captchaService.GenerateCaptchaCode();
                HttpContext.Session.SetString("CaptchaCode", captchaCode);
                return View(model);
            }

            // اعتبارسنجی کپچا
            var sessionCaptcha = HttpContext.Session.GetString("CaptchaCode");
            if (!_captchaService.ValidateCaptcha(model.CaptchaCode, sessionCaptcha))
            {
                ModelState.AddModelError("", "کد امنیتی وارد شده صحیح نیست.");
                // تولید کپچای جدید در صورت خطا
                var newCaptchaCode = _captchaService.GenerateCaptchaCode();
                HttpContext.Session.SetString("CaptchaCode", newCaptchaCode);
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);
            if (result.Succeeded)
            {
                _logger.LogInformation("User {Email} logged in.", model.Email);
                return RedirectToAction("Index", "Home");
            }
            else if (result.IsLockedOut)
            {
                _logger.LogWarning("User account {Email} locked out.", model.Email);
                ModelState.AddModelError("", "حساب کاربری شما قفل شده است. لطفاً چند دقیقه دیگر مجدداً تلاش کنید.");
            }
            else if (result.IsNotAllowed)
            {
                _logger.LogWarning("User {Email} not allowed to log in.", model.Email);
                ModelState.AddModelError("", "ورود به حساب کاربری شما مجاز نیست.");
            }
            else
            {
                _logger.LogWarning("Invalid login attempt for user {Email}.", model.Email);
                ModelState.AddModelError("", "نام کاربری یا رمز عبور اشتباه است.");
            }

            // تولید کپچای جدید در صورت خطا
            var captchaCodeNew = _captchaService.GenerateCaptchaCode();
            HttpContext.Session.SetString("CaptchaCode", captchaCodeNew);

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            return RedirectToAction("Login");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GenerateCaptcha()
        {
            var captchaCode = _captchaService.GenerateCaptchaCode();
            HttpContext.Session.SetString("CaptchaCode", captchaCode);

            var captchaImage = _captchaService.GenerateCaptchaImage(captchaCode);

            return File(captchaImage, "image/png");
        }
    }
}