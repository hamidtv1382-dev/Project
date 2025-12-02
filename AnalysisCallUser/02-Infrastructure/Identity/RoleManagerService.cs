using AnalysisCallUser._01_Domain.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AnalysisCallUser._02_Infrastructure.Identity
{
    public class RoleManagerService
    {
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ILogger<RoleManagerService> _logger;

        public RoleManagerService(RoleManager<ApplicationRole> roleManager, ILogger<RoleManagerService> logger)
        {
            _roleManager = roleManager;
            _logger = logger;
        }

        public async Task<bool> CreateRoleAsync(string roleName, string description)
        {
            if (await _roleManager.RoleExistsAsync(roleName))
            {
                _logger.LogWarning($"Role {roleName} already exists.");
                return false;
            }

            var role = new ApplicationRole
            {
                Name = roleName,
                Description = description,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _roleManager.CreateAsync(role);

            if (result.Succeeded)
            {
                _logger.LogInformation($"Role {roleName} created successfully.");
                return true;
            }

            _logger.LogError($"Error creating role {roleName}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            return false;
        }

        public async Task<List<ApplicationRole>> GetAllRolesAsync()
        {
            return await _roleManager.Roles.ToListAsync();
        }

        public async Task<ApplicationRole> FindRoleByNameAsync(string roleName)
        {
            return await _roleManager.FindByNameAsync(roleName);
        }
    }
}
