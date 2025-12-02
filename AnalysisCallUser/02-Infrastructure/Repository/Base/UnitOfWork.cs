using AnalysisCallUser._01_Domain.Core.Contracts;
using AnalysisCallUser._02_Infrastructure.Data;
using AnalysisCallUser._02_Infrastructure.Repository.Repositories;

namespace AnalysisCallUser._02_Infrastructure.Repository.Base
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private ICallDetailRepository _callDetails;
        private ICallTypeRepository _callTypes;
        private ICountryRepository _countries;
        private ICityRepository _cities;
        private IOperatorRepository _operators;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        public ICallDetailRepository CallDetails => _callDetails ??= new CallDetailRepository(_context);
        public ICallTypeRepository CallTypes => _callTypes ??= new CallTypeRepository(_context);
        public ICountryRepository Countries => _countries ??= new CountryRepository(_context);
        public ICityRepository Cities => _cities ??= new CityRepository(_context);
        public IOperatorRepository Operators => _operators ??= new OperatorRepository(_context);

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
