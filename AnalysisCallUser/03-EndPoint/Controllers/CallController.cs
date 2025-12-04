using AnalysisCallUser._01_Domain.Core.Contracts;
using AnalysisCallUser._01_Domain.Core.DTOs;
using AnalysisCallUser._02_Infrastructure.Data;
using AnalysisCallUser._02_Infrastructure.Helpers;
using AnalysisCallUser._03_EndPoint.Models.ViewModels.Call;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AnalysisCallUser._03_EndPoint.Controllers
{

    [Authorize]
    public class CallController : Controller
    {
        private readonly ICallDetailRepository _callDetailRepository;
        // نیازی به تزریق ریپازیتوری کشور و شهر نداریم، چون مستقیما از Context استفاده می‌کنیم
        private readonly AppDbContext _context;

        // تزریق AppDbContext برای دسترسی مستقیم به Countries و Cities
        public CallController(ICallDetailRepository callDetailRepository, AppDbContext context)
        {
            _callDetailRepository = callDetailRepository;
            _context = context;
        }

        // ... متدهای قبلی ...

        // GET: /Call/Search
        [HttpGet]
        public async Task<IActionResult> Search()
        {
            var model = new CallSearchViewModel
            {
                Filter = new CallFilterViewModel(),
                // بارگذاری لیست کشورها برای نمایش در کشویی
                Countries = await _context.Countries.OrderBy(c => c.CountryName).ToListAsync()
            };
            return View(model);
        }

        // POST: /Call/Search
        [HttpPost]
        public async Task<IActionResult> Search(CallSearchViewModel model)
        {
            var callFilterDto = new CallFilterDto
            {
                // ... فیلدهای قبلی ...
                ANumber = model.Filter.ANumber,
                BNumber = model.Filter.BNumber,
                Answer = model.Filter.Answer,
                Page = model.Filter.Page,
                PageSize = model.Filter.PageSize,

                // فیلدهای جدید
                OriginCountryID = model.Filter.OriginCountryID,
                OriginCityID = model.Filter.OriginCityID,
                DestCountryID = model.Filter.DestCountryID,
                DestCityID = model.Filter.DestCityID
            };

            var data = await _callDetailRepository.GetFilteredAsync(callFilterDto);
            var count = await _callDetailRepository.GetFilteredCountAsync(callFilterDto);

            var callDetailDtos = data.Select(cd => new CallDetailDto
            {
                DetailID = cd.DetailID,
                ANumber = cd.ANumber,
                BNumber = cd.BNumber,
                AccountingTime = cd.AccountingTime,
                Length = cd.Length,
                OriginCountryName = cd.OriginCountry?.CountryName,
                OriginCityName = cd.OriginCity?.CityName,
                DestCountryName = cd.DestCountry?.CountryName,
                DestCityName = cd.DestCity?.CityName,
                Answer = cd.Answer
            }).ToList();

            model.Results = new PagedResult<CallDetailDto>(callDetailDtos, count, model.Filter.Page, model.Filter.PageSize);

            // پر کردن مجدد لیست‌ها برای POST-back
            model.Countries = await _context.Countries.OrderBy(c => c.CountryName).ToListAsync();
            if (model.Filter.OriginCountryID.HasValue)
            {
                model.OriginCities = await _context.Cities.Where(c => c.CountryID == model.Filter.OriginCountryID.Value).ToListAsync();
            }
            if (model.Filter.DestCountryID.HasValue)
            {
                model.DestCities = await _context.Cities.Where(c => c.CountryID == model.Filter.DestCountryID.Value).ToListAsync();
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_SearchResults", model.Results);
            }

            return View(model);
        }

        // GET: /Call/Details/5 (بدون تغییر)
        public async Task<IActionResult> Details(int id)
        {
            var call = await _callDetailRepository.GetByIdAsync(id);
            if (call == null)
            {
                return NotFound();
            }

            var viewModel = new CallDetailsViewModel(new CallDetailDto
            {
                DetailID = call.DetailID,
                ANumber = call.ANumber,
                BNumber = call.BNumber,
                AccountingTime = call.AccountingTime,
                Length = call.Length,
                OriginCountryName = call.OriginCountry?.CountryName,
                DestCountryName = call.DestCountry?.CountryName,
                Answer = call.Answer
            });

            return View(viewModel);
        }

        // +++ متدهای جدید برای بارگذاری AJAX +++

        [HttpGet]
        public async Task<JsonResult> GetCountries()
        {
            var countries = await _context.Countries
                                          .OrderBy(c => c.CountryName)
                                          .ToListAsync();
            return Json(countries);
        }

        [HttpGet]
        public async Task<JsonResult> GetCities(int countryId)
        {
            var cities = await _context.Cities
                                       .Where(c => c.CountryID == countryId)
                                       .OrderBy(c => c.CityName)
                                       .ToListAsync();
            return Json(cities);
        }
    }
}
