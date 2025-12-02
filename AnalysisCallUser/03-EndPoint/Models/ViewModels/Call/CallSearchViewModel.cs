using AnalysisCallUser._01_Domain.Core.DTOs;
using AnalysisCallUser._02_Infrastructure.Helpers;

namespace AnalysisCallUser._03_EndPoint.Models.ViewModels.Call
{
    public class CallSearchViewModel
    {
        public CallFilterViewModel Filter { get; set; }
        public PagedResult<CallDetailDto> Results { get; set; }
    }
}
