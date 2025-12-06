using AnalysisCallUser._01_Domain.Core.DTOs;
using AnalysisCallUser._01_Domain.Core.Entities;
using AnalysisCallUser._02_Infrastructure.Helpers;

namespace AnalysisCallUser._03_EndPoint.Models.ViewModels.Call
{
    public class CallSearchViewModel
    {
        public CallFilterViewModel Filter { get; set; }
        public PagedResult<CallDetailDto> Results { get; set; }
        public IEnumerable<Country> Countries { get; set; } = new List<Country>();
        public IEnumerable<City> OriginCities { get; set; } = new List<City>();
        public IEnumerable<City> DestCities { get; set; } = new List<City>();
        public List<Operator> OriginOperators { get; set; }
        public List<Operator> DestOperators { get; set; }
    }
}
