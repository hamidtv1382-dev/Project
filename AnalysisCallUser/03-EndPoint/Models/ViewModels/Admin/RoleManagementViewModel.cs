using AnalysisCallUser._01_Domain.Core.DTOs;
using AnalysisCallUser._01_Domain.Core.Entities;

namespace AnalysisCallUser._03_EndPoint.Models.ViewModels.Admin
{
    //public class RoleManagementViewModel
    //{
    //    public List<ApplicationRole> Roles { get; set; }
    //    public List<ApplicationUser> Users { get; set; }
    //}
    public class RoleManagementViewModel
    {
        public List<ApplicationRole> Roles { get; set; }
        public List<UserWithRolesDto> UsersWithRoles { get; set; }
    }
}
