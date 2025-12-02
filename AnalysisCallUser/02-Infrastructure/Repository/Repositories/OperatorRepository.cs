using AnalysisCallUser._01_Domain.Core.Contracts;
using AnalysisCallUser._01_Domain.Core.Entities;
using AnalysisCallUser._02_Infrastructure.Data;
using AnalysisCallUser._02_Infrastructure.Repository.Base;

namespace AnalysisCallUser._02_Infrastructure.Repository.Repositories
{
    public class OperatorRepository : Repository<Operator>, IOperatorRepository
    {
        public OperatorRepository(AppDbContext context) : base(context)
        {
        }
        public IQueryable<Operator> GetAll()
        {
            return _context.Operators;
        }
    }
}
