using AnalysisCallUser._01_Domain.Core.Contracts;
using AnalysisCallUser._01_Domain.Core.DTOs;
using AnalysisCallUser._01_Domain.Core.Entities;
using AnalysisCallUser._01_Domain.Core.Enums;
using AnalysisCallUser._02_Infrastructure.Data;
using AnalysisCallUser._02_Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;

namespace AnalysisCallUser._01_Domain.Services
{
    public class ExportService : IExportService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly AppDbContext _context; 

        public ExportService(IUnitOfWork unitOfWork, AppDbContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context; 
        }

        public async Task<ExportHistory> CreateExportRequestAsync(ExportRequestDto model, int userId)
        {
            var exportHistory = new ExportHistory
            {
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                ExportType = model.ExportType,
                FilterParameters = System.Text.Json.JsonSerializer.Serialize(model.Filter),
                RecordCount = model.TotalRecords,
                IsCompleted = false,
                IsSuccessful = false
            };

            await _context.Set<ExportHistory>().AddAsync(exportHistory);
            await _unitOfWork.CompleteAsync();

            return exportHistory;
        }

        public async Task<byte[]> GenerateExportFileAsync(ExportHistory exportRequest)
        {
            var filterOptions = System.Text.Json.JsonSerializer.Deserialize<CallFilterDto>(exportRequest.FilterParameters);

            var dataToExport = await _unitOfWork.CallDetails.GetFilteredAsync(filterOptions);

            switch (exportRequest.ExportType)
            {
                case ExportType.CSV:
                    return ExportHelper.GenerateCsv(dataToExport);
                default:
                    throw new NotSupportedException("Export type not supported.");
            }
        }

        public async Task<IEnumerable<ExportHistory>> GetUserExportHistoryAsync(int userId)
        {
            return await _context.Set<ExportHistory>()
                                .Where(eh => eh.UserId == userId)
                                .OrderByDescending(eh => eh.CreatedAt)
                                .ToListAsync();
        }
    }
}

