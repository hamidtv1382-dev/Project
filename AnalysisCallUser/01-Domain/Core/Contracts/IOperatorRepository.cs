using AnalysisCallUser._01_Domain.Core.Entities;

namespace AnalysisCallUser._01_Domain.Core.Contracts
{
    public interface IOperatorRepository : IRepository<Operator>
    {
        IQueryable<Operator> GetAll();

    }
}
