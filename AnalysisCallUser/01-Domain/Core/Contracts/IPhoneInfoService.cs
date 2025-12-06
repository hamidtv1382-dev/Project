using AnalysisCallUser._01_Domain.Core.Entities;

namespace AnalysisCallUser._01_Domain.Core.Contracts
{
    public interface IPhoneInfoService
    {
        Task<(Country Country, City City, Operator Operator)> GetPhoneInfoAsync(string phoneNumber);
    }
}
