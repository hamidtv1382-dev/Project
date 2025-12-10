using AnalysisCallUser._01_Domain.Core.DTOs;
using AnalysisCallUser._01_Domain.Core.Entities;
using AnalysisCallUser._02_Infrastructure.Helpers;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace AnalysisCallUser._03_EndPoint.Models.ViewModels.Call
{
    public class CallSearchViewModel
    {
        [BindNever] // <<<< اضافه شده
        public PagedResult<CallDetailDto>? Results { get; set; }

        public CallFilterViewModel? Filter { get; set; }

        [BindNever] // <<<< اضافه شده
        public List<Country>? Countries { get; set; }

        [BindNever] // <<<< اضافه شده
        public List<City>? OriginCities { get; set; }

        [BindNever] // <<<< اضافه شده
        public List<Operator>? OriginOperators { get; set; }

        [BindNever] // <<<< اضافه شده
        public List<City>? DestCities { get; set; }

        [BindNever] // <<<< اضافه شده
        public List<Operator>? DestOperators { get; set; }
    }
}
