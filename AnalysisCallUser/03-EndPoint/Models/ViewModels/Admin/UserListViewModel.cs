using AnalysisCallUser._01_Domain.Core.DTOs;
using AnalysisCallUser._02_Infrastructure.Helpers;

namespace AnalysisCallUser._03_EndPoint.Models.ViewModels.Admin
{
    public class UserListViewModel
    {
        public PagedResult<UserDto> Users { get; set; }
    }
}
