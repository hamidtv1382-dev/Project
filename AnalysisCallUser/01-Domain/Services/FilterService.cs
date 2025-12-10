using AnalysisCallUser._01_Domain.Core.Contracts;
using AnalysisCallUser._01_Domain.Core.DTOs;
using AnalysisCallUser._01_Domain.Core.Entities;
using AnalysisCallUser._02_Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnalysisCallUser._01_Domain.Services
{
    public class FilterService : IFilterService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly AppDbContext _context;

        public FilterService(IUnitOfWork unitOfWork, AppDbContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context; 
        }

        public async Task SaveFilterAsync(FilterHistory filter)
        {
            await _context.Set<FilterHistory>().AddAsync(filter);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<IEnumerable<FilterHistory>> GetUserFilterHistoryAsync(int userId)
        {
            return await _context.Set<FilterHistory>()
                                .Where(fh => fh.UserId == userId)
                                .OrderByDescending(fh => fh.CreatedAt)
                                .ToListAsync();
        }

        public Task<CallFilterDto> GetDefaultFilterAsync()
        {
            return Task.FromResult(new CallFilterDto { Page = 1, PageSize = 50 });
        }
    }
}