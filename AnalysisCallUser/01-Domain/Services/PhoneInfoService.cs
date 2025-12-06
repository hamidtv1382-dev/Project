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

            // جستجو در جدول CallDetails برای یافتن رکوردی که با شماره مطابقت دارد
            var callDetail = await _context.CallDetails
                .Where(cd => cd.ANumber == phoneNumber || cd.BNumber == phoneNumber)
                // فقط اطلاعات مورد نیاز را Include می‌کنیم تا کوئری بهینه باشد
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

            // بررسی می‌کنیم که شماره وارد شده به عنوان مبدأ بوده یا مقصد
            // و اطلاعات مربوطه را برمی‌گردانیم
            if (callDetail.ANumber == phoneNumber)
            {
                return (callDetail.OriginCountry, callDetail.OriginCity, callDetail.OriginOperator);
            }
            else // if (callDetail.BNumber == phoneNumber)
            {
                return (callDetail.DestCountry, callDetail.DestCity, callDetail.DestOperator);
            }
        }
    }
}
