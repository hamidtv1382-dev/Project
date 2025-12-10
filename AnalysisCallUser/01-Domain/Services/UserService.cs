using AnalysisCallUser._01_Domain.Core.Contracts;
using AnalysisCallUser._01_Domain.Core.DTOs;
using AnalysisCallUser._01_Domain.Core.Entities;
using AnalysisCallUser._01_Domain.Core.Enums;
using AnalysisCallUser._03_EndPoint.Models.ViewModels.Admin;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static AnalysisCallUser._01_Domain.Core.DTOs.OperatorPerformanceDto;

namespace AnalysisCallUser._01_Domain.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ILogger<UserService> _logger;

        public UserService(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, ILogger<UserService> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        public async Task<ApplicationUser> FindUserByIdAsync(int userId)
        {
            return await _userManager.FindByIdAsync(userId.ToString());
        }

        public async Task<ApplicationUser> FindUserByNameAsync(string userName)
        {
            return await _userManager.FindByNameAsync(userName);
        }
        public async Task<List<UserWithRolesDto>> GetUsersWithRolesAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            var usersWithRolesDto = new List<UserWithRolesDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                usersWithRolesDto.Add(new UserWithRolesDto
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Roles = roles.ToList()
                });
            }

            return usersWithRolesDto;
        }
        public async Task<bool> CreateUserAsync(CreateUserDto model, string roleName)
        {
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                PhoneNumber = model.PhoneNumber,
                EmailConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                _logger.LogError("Error creating user {Email}: {Errors}", model.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
                return false;
            }

            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                _logger.LogWarning("Role {RoleName} does not exist.", roleName);
                return false;
            }

            await _userManager.AddToRoleAsync(user, roleName);
            _logger.LogInformation("User {Email} created and added to role {RoleName}.", model.Email, roleName);
            return true;
        }

        public async Task<ProfileDto> GetUserProfileAsync(int userId)
        {
            var user = await FindUserByIdAsync(userId);
            if (user == null) return null;

            var roles = await _userManager.GetRolesAsync(user);
            var firstRoleName = roles.FirstOrDefault();

            UserRole userRole = UserRole.User; 
            if (!string.IsNullOrEmpty(firstRoleName) && Enum.TryParse<UserRole>(firstRoleName, true, out var parsedRole))
            {
                userRole = parsedRole;
            }

            return new ProfileDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                Role = userRole
            };
        }

        public async Task<bool> UpdateUserProfileAsync(EditProfileDto model)
        {
            var user = await FindUserByIdAsync(model.Id);
            if (user == null) return false;

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.PhoneNumber = model.PhoneNumber;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                _logger.LogInformation("User profile for {UserId} updated.", model.Id);
                return true;
            }

            _logger.LogError("Error updating user profile for {UserId}: {Errors}", model.Id, string.Join(", ", result.Errors.Select(e => e.Description)));
            return false;
        }

        public async Task<IEnumerable<UserLoginHistory>> GetUserLoginHistoryAsync(int userId)
        {
           
            return new List<UserLoginHistory>();
        }
    }
}