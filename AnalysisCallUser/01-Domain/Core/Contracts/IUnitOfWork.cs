namespace AnalysisCallUser._01_Domain.Core.Contracts
{
    public interface IUnitOfWork : IDisposable
    {
        ICallDetailRepository CallDetails { get; }
        ICallTypeRepository CallTypes { get; }
        ICountryRepository Countries { get; }
        ICityRepository Cities { get; }
        IOperatorRepository Operators { get; }

        Task<int> CompleteAsync();
    }
}
