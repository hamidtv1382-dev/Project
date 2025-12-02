using AnalysisCallUser._01_Domain.Core.Contracts;
using AnalysisCallUser._01_Domain.Core.DTOs;
using AnalysisCallUser._02_Infrastructure.Helpers;
using AnalysisCallUser._03_EndPoint.Models.ViewModels.Call;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AnalysisCallUser._03_EndPoint.Controllers
{

    [Authorize]
    public class CallController : Controller
    {
        private readonly ICallDetailRepository _callDetailRepository;

        public CallController(ICallDetailRepository callDetailRepository)
        {
            _callDetailRepository = callDetailRepository;
        }

        public IActionResult Search()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Search(CallFilterViewModel filter)
        {
            // تبدیل ViewModel به DTO
            var callFilterDto = new CallFilterDto
            {
                StartDate = filter.StartDate,
                EndDate = filter.EndDate,
                StartTime = filter.StartTime,
                EndTime = filter.EndTime,
                ANumber = filter.ANumber,
                BNumber = filter.BNumber,
                OriginCountryID = filter.OriginCountryID,
                DestCountryID = filter.DestCountryID,
                OriginCityID = filter.OriginCityID,
                DestCityID = filter.DestCityID,
                OriginOperatorID = filter.OriginOperatorID,
                DestOperatorID = filter.DestOperatorID,
                TypeID = filter.TypeID,
                Answer = filter.Answer,
                Page = filter.Page,
                PageSize = filter.PageSize
            };

            var data = await _callDetailRepository.GetFilteredAsync(callFilterDto);
            var count = await _callDetailRepository.GetFilteredCountAsync(callFilterDto);

            // تبدیل Entity به DTO برای نمایش در View
            var callDetailDtos = data.Select(cd => new CallDetailDto
            {
                DetailID = cd.DetailID,
                ANumber = cd.ANumber,
                BNumber = cd.BNumber,
                AccountingTime_Date = cd.AccountingTime_Date,
                AccountingTime_Time = cd.AccountingTime_Time,
                Length = cd.Length,
                OriginCountryName = cd.OriginCountry.CountryName,
                DestCountryName = cd.DestCountry.CountryName,
                Answer = cd.Answer
            }).ToList();

            var result = new CallSearchViewModel
            {
                Filter = filter,
                Results = new PagedResult<CallDetailDto>(callDetailDtos, count, filter.Page, filter.PageSize)
            };

            return View(result);
        }

        public async Task<IActionResult> Details(int id)
        {
            var call = await _callDetailRepository.GetByIdAsync(id);
            if (call == null) return NotFound();

            // تبدیل Entity به DTO برای نمایش در View
            var viewModel = new CallDetailsViewModel(new CallDetailDto
            {
                DetailID = call.DetailID,
                ANumber = call.ANumber,
                BNumber = call.BNumber,
                AccountingTime_Date = call.AccountingTime_Date,
                AccountingTime_Time = call.AccountingTime_Time,
                Length = call.Length,
                OriginCountryName = call.OriginCountry.CountryName,
                DestCountryName = call.DestCountry.CountryName,
                Answer = call.Answer
            });

            return View(viewModel);
        }
    }
}
