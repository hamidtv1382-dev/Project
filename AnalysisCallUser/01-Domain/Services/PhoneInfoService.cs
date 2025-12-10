using AnalysisCallUser._01_Domain.Core.Contracts;
using AnalysisCallUser._01_Domain.Core.Entities;
using AnalysisCallUser._02_Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AnalysisCallUser._01_Domain.Services
{
    public class PhoneInfoService : IPhoneInfoService
    {
        private readonly AppDbContext _context;

        public PhoneInfoService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(Country Country, City City, Operator Operator)> GetPhoneInfoAsync(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return (null, null, null);
            }

            var callDetail = await _context.CallDetails
                .Where(cd => cd.ANumber == phoneNumber || cd.BNumber == phoneNumber)
                .Include(cd => cd.OriginCountry)
                .Include(cd => cd.OriginCity)
                .Include(cd => cd.OriginOperator)
                .Include(cd => cd.DestCountry)
                .Include(cd => cd.DestCity)
                .Include(cd => cd.DestOperator)
                .FirstOrDefaultAsync();

            if (callDetail == null)
            {
                return (null, null, null);
            }

            if (callDetail.ANumber == phoneNumber)
            {
                return (callDetail.OriginCountry, callDetail.OriginCity, callDetail.OriginOperator);
            }
            else 
            {
                return (callDetail.DestCountry, callDetail.DestCity, callDetail.DestOperator);
            }
        }
    }
}
