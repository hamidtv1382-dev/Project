using AnalysisCallUser._01_Domain.Core.DTOs;
using AnalysisCallUser._01_Domain.Core.Entities;

namespace AnalysisCallUser._03_EndPoint.Models.ViewModels.Export
{
    public class ExportHistoryViewModel
    {
        public List<ExportHistoryDto> Exports { get; set; }
    }
}
