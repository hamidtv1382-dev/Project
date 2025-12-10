using AnalysisCallUser._01_Domain.Core.Contracts;
using AnalysisCallUser._03_EndPoint.Models.ViewModels.Profile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static AnalysisCallUser._01_Domain.Core.DTOs.OperatorPerformanceDto;

namespace AnalysisCallUser._03_EndPoint.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly IUserService _userService;

        public ProfileController(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
            var data = await _userService.GetUserProfileAsync(userId);
            return View(data);
        }

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
            var user = await _userService.FindUserByIdAsync(userId);
            var viewModel = new EditProfileViewModel { Id = user.Id, FirstName = user.FirstName, LastName = user.LastName, PhoneNumber = user.PhoneNumber };
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditProfileViewModel model)
        {
            if (ModelState.IsValid)
            {
                var editProfileDto = new EditProfileDto
                {
                    Id = model.Id,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    PhoneNumber = model.PhoneNumber
                };

                await _userService.UpdateUserProfileAsync(editProfileDto);
                return RedirectToAction("Index");
            }
            return View(model);
        }

        public async Task<IActionResult> LoginHistory()
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
            var history = await _userService.GetUserLoginHistoryAsync(userId);
            return View(history);
        }
    }
}
