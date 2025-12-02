using AnalysisCallUser._01_Domain.Core.DTOs;
using AnalysisCallUser._01_Domain.Core.Entities;

namespace AnalysisCallUser._01_Domain.Core.Contracts
{
    public interface IExportService
    {
        Task<ExportHistory> CreateExportRequestAsync(ExportRequestDto model, int userId);
        Task<byte[]> GenerateExportFileAsync(ExportHistory exportRequest);
        Task<IEnumerable<ExportHistory>> GetUserExportHistoryAsync(int userId);
    }
}