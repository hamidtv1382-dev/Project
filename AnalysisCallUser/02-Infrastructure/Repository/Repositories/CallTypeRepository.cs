using AnalysisCallUser._01_Domain.Core.Contracts;
using AnalysisCallUser._02_Infrastructure.Data;
using AnalysisCallUser._02_Infrastructure.Repository.Base;
using AnalysisCallUser._01_Domain.Core.Entities;

namespace AnalysisCallUser._02_Infrastructure.Repository.Repositories
{
    public class CallTypeRepository : Repository<CallType>, ICallTypeRepository
    {
        public CallTypeRepository(AppDbContext context) : base(context)
        {
        }
    }
}
