using AnalysisCallUser._01_Domain.Core.DTOs;
using AnalysisCallUser._01_Domain.Core.Entities;
using AnalysisCallUser._03_EndPoint.Models.ViewModels.Admin;
using static AnalysisCallUser._01_Domain.Core.DTOs.OperatorPerformanceDto;

namespace AnalysisCallUser._01_Domain.Core.Contracts
{
    public interface IUserService
    {
        Task<ApplicationUser> FindUserByIdAsync(int userId);
        Task<ApplicationUser> FindUserByNameAsync(string userName);
        Task<bool> CreateUserAsync(CreateUserDto model, string roleName);
        Task<ProfileDto> GetUserProfileAsync(int userId);
        Task<bool> UpdateUserProfileAsync(EditProfileDto model);
        Task<IEnumerable<UserLoginHistory>> GetUserLoginHistoryAsync(int userId);
        Task<List<UserWithRolesDto>> GetUsersWithRolesAsync();

    }
}