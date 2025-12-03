using AnalysisCallUser._01_Domain.Core.Contracts;
using AnalysisCallUser._01_Domain.Core.DTOs;
using AnalysisCallUser._01_Domain.Core.Entities;
using AnalysisCallUser._01_Domain.Core.Enums; // برای دسترسی به enum UserRole
using AnalysisCallUser._02_Infrastructure.Helpers; // برای دسترسی به PagedResult
using AnalysisCallUser._03_EndPoint.Models.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnalysisCallUser._03_EndPoint.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class AdminController : Controller
    {
        private readonly IUserService _userService;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(IUserService userService, RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            _userService = userService;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        // متد اصلاح شده برای دریافت و ارسال لیست کاربران به صورت UserDto
        public async Task<IActionResult> Users(string search, string role, string status, int pageNumber = 1, int pageSize = 10)
        {
           

            // 1. دریافت تمام کاربران به صورت IQueryable برای اعمال فیلترها
            var usersQuery = _userManager.Users.AsQueryable();

            // 2. اعمال فیلترها
            if (!string.IsNullOrEmpty(search))
            {
                usersQuery = usersQuery.Where(u =>
                    u.UserName.Contains(search) ||
                    u.Email.Contains(search) ||
                    (u.FirstName + " " + u.LastName).Contains(search));
            }

            if (!string.IsNullOrEmpty(role))
            {
                var usersInRole = await _userManager.GetUsersInRoleAsync(role);
                var userIdsInRole = usersInRole.Select(u => u.Id);
                usersQuery = usersQuery.Where(u => userIdsInRole.Contains(u.Id));
            }

            if (status == "active")
            {
                usersQuery = usersQuery.Where(u => u.IsActive);
            }
            else if (status == "inactive")
            {
                usersQuery = usersQuery.Where(u => !u.IsActive);
            }

            // 3. استفاده از PaginationHelper برای صفحه‌بندی کاربران از دیتابیس
            // این کار دو مرحله اصلی را انجام می‌دهد: شمارش کل و دریافت صفحه مورد نظر
            var pagedApplicationUsers = PaginationHelper.CreatePagedResult(
                usersQuery.OrderBy(u => u.UserName),
                pageNumber,
                pageSize
            );

            // 4. تبدیل لیست ApplicationUser در صفحه فعلی به لیست UserDto
            var userDtos = new List<UserDto>();
            foreach (var user in pagedApplicationUsers.Data)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var primaryRoleString = roles.FirstOrDefault() ?? "User";

                if (!Enum.TryParse<UserRole>(primaryRoleString, true, out var userRoleEnum))
                {
                    userRoleEnum = UserRole.User;
                }

                userDtos.Add(new UserDto
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    PhoneNumber = user.PhoneNumber,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                    LastLoginAt = null, // TODO: از جدول تاریخچه ورود پر شود
                    Role = userRoleEnum
                });
            }

            // 5. ساخت نهایی PagedResult<UserDto> با استفاده از اطلاعات صفحه‌بندی از مرحله 3
            var finalPagedResult = new PagedResult<UserDto>(
                userDtos,
                pagedApplicationUsers.TotalItems,
                pagedApplicationUsers.PageNumber,
                pagedApplicationUsers.PageSize
            );

            // 6. ارسال به View
            var viewModel = new UserListViewModel
            {
                Users = finalPagedResult
            };

            return View(viewModel);
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