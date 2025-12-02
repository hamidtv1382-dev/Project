using AnalysisCallUser._01_Domain.Core.Contracts;
using AnalysisCallUser._01_Domain.Core.DTOs;
using AnalysisCallUser._01_Domain.Core.Entities;
using AnalysisCallUser._03_EndPoint.Models.ViewModels.Admin;
using AnalysisCallUser._03_EndPoint.Models.ViewModels.Profile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static AnalysisCallUser._01_Domain.Core.DTOs.OperatorPerformanceDto;

namespace AnalysisCallUser._03_EndPoint.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly IUserService _userService;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public AdminController(IUserService userService, RoleManager<ApplicationRole> roleManager)
        {
            _roleManager = roleManager;

            _userService = userService;
        }

        public async Task<IActionResult> Users()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> RoleManagement()
        {
            var model = new RoleManagementViewModel
            {
                Roles = await _roleManager.Roles.ToListAsync(),
                UsersWithRoles = await _userService.GetUsersWithRolesAsync()
            };
            return View(model);
        }
        [HttpGet]
        [Authorize(Roles = "SuperAdmin")]
        public IActionResult AddAdmin()
        {
            var model = new AddUserViewModel { Role = "Admin" };
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> AddAdmin(AddUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var createUserDto = new CreateUserDto
                {
                    Email = model.Email,
                    Password = model.Password,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    PhoneNumber = model.PhoneNumber
                };

                await _userService.CreateUserAsync(createUserDto, "Admin");
                return RedirectToAction("Users");
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult AddUser()
        {
            var model = new AddUserViewModel { Role = "User" };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddUser(AddUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var createUserDto = new CreateUserDto
                {
                    Email = model.Email,
                    Password = model.Password,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    PhoneNumber = model.PhoneNumber
                };

                await _userService.CreateUserAsync(createUserDto, "User");
                return RedirectToAction("Users");
            }
            return View(model);
        }
    }
}
