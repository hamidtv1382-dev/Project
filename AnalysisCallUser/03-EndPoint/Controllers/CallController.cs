using AnalysisCallUser._01_Domain.Core.Contracts;
using AnalysisCallUser._01_Domain.Core.DTOs;
using AnalysisCallUser._01_Domain.Services;
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
        private readonly AppDbContext _context;
        private readonly IPhoneInfoService _phoneInfoService;
        public CallController(ICallDetailRepository callDetailRepository, AppDbContext context, IPhoneInfoService phoneInfoService)
        {
            _callDetailRepository = callDetailRepository;
            _context = context;
            _phoneInfoService = phoneInfoService;
        }

        // GET: /Call/Search
        [HttpGet]
        public async Task<IActionResult> Search()
        {
            var model = new CallSearchViewModel
            {
                Filter = new CallFilterViewModel(),
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

                // فیلدهای کشور و شهر
                OriginCountryID = model.Filter.OriginCountryID,
                OriginCityID = model.Filter.OriginCityID,
                DestCountryID = model.Filter.DestCountryID,
                DestCityID = model.Filter.DestCityID,

                // +++ فیلدهای جدید اپراتور +++
                OriginOperatorID = model.Filter.OriginOperatorID,
                DestOperatorID = model.Filter.DestOperatorID
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
                // +++ نام اپراتورها برای نمایش +++
                OriginOperatorName = cd.OriginOperator?.OperatorName,
                DestOperatorName = cd.DestOperator?.OperatorName,
                Answer = cd.Answer
            }).ToList();

            model.Results = new PagedResult<CallDetailDto>(callDetailDtos, count, model.Filter.Page, model.Filter.PageSize);

            // پر کردن مجدد لیست‌ها برای POST-back
            model.Countries = await _context.Countries.OrderBy(c => c.CountryName).ToListAsync();
            if (model.Filter.OriginCountryID.HasValue)
            {
                model.OriginCities = await _context.Cities.Where(c => c.CountryID == model.Filter.OriginCountryID.Value).ToListAsync();
                // +++ بارگذاری اپراتورهای مبدأ +++
                model.OriginOperators = await _context.Operators.Where(o => o.CountryID == model.Filter.OriginCountryID.Value).ToListAsync();
            }
            if (model.Filter.DestCountryID.HasValue)
            {
                model.DestCities = await _context.Cities.Where(c => c.CountryID == model.Filter.DestCountryID.Value).ToListAsync();
                // +++ بارگذاری اپراتورهای مقصد +++
                model.DestOperators = await _context.Operators.Where(o => o.CountryID == model.Filter.DestCountryID.Value).ToListAsync();
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_SearchResults", model.Results);
            }

            return View(model);
        }

        // GET: /Call/Details/5
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
                // +++ نام اپراتورها برای جزئیات +++
                OriginOperatorName = call.OriginOperator?.OperatorName,
                DestOperatorName = call.DestOperator?.OperatorName,
                Answer = call.Answer
            });

            return View(viewModel);
        }

        // +++ اکشن متد جدید برای بارگذاری AJAX اپراتورها +++
        [HttpGet]
        public async Task<JsonResult> GetOperators(int countryId)
        {
            var operators = await _context.Operators
                                       .Where(o => o.CountryID == countryId)
                                       .OrderBy(o => o.OperatorName)
                                       .ToListAsync();
            return Json(operators);
        }

        // متدهای قبلی برای بارگذاری شهرها و کشورها بدون تغییر باقی می‌مانند
        [HttpGet]
        public async Task<JsonResult> GetCountries()
        {
            var countries = await _context.Countries
                                          .OrderBy(c => c.CountryName)
                                          .ToListAsync();
            return Json(countries);
        }
        public async Task<JsonResult> GetPhoneInfo(string number)
        {
            var (country, city, op) = await _phoneInfoService.GetPhoneInfoAsync(number);

            // فقط IDها را برای ارسال به کلاینت برمی‌گردانیم
            return Json(new
            {
                success = (country != null), // یک فلگ برای موفقیت‌آمیز بودن یا نبودن
                countryId = country?.CountryID,
                cityId = city?.CityID,
                operatorId = op?.OperatorID
            });
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
        [HttpPost]
        public async Task<IActionResult> ExportSearchResults(CallSearchViewModel model)
        {
            var callFilterDto = new CallFilterDto
            {
                ANumber = model.Filter.ANumber,
                BNumber = model.Filter.BNumber,
                Answer = model.Filter.Answer,
                StartDate = model.Filter.StartDate,
                EndDate = model.Filter.EndDate,
                OriginCountryID = model.Filter.OriginCountryID,
                OriginCityID = model.Filter.OriginCityID,
                DestCountryID = model.Filter.DestCountryID,
                DestCityID = model.Filter.DestCityID,
                OriginOperatorID = model.Filter.OriginOperatorID,
                DestOperatorID = model.Filter.DestOperatorID,
                // برای اکسپورت، تمام رکوردها را دریافت می‌کنیم
                Page = 1,
                PageSize = int.MaxValue
            };

            var data = await _callDetailRepository.GetFilteredAsync(callFilterDto);

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
                OriginOperatorName = cd.OriginOperator?.OperatorName,
                DestOperatorName = cd.DestOperator?.OperatorName,
                Answer = cd.Answer
            }).ToList();

            var csv = ExportHelper.GenerateCsv(callDetailDtos);
            var fileName = $"CallSearchResults_{DateTime.Now:yyyyMMddHHmmss}.csv";

            return File(csv, "text/csv", fileName);
        }

        // GET: /Call/ExportDetails/5
        [HttpGet]
        public async Task<IActionResult> ExportDetails(int id)
        {
            var call = await _callDetailRepository.GetByIdAsync(id);
            if (call == null)
            {
                return NotFound();
            }

            var callDetailDto = new CallDetailDto
            {
                DetailID = call.DetailID,
                ANumber = call.ANumber,
                BNumber = call.BNumber,
                AccountingTime = call.AccountingTime,
                Length = call.Length,
                OriginCountryName = call.OriginCountry?.CountryName,
                OriginCityName = call.OriginCity?.CityName,
                DestCountryName = call.DestCountry?.CountryName,
                DestCityName = call.DestCity?.CityName,
                OriginOperatorName = call.OriginOperator?.OperatorName,
                DestOperatorName = call.DestOperator?.OperatorName,
                Answer = call.Answer
            };

            var csv = ExportHelper.GenerateCsv(new List<CallDetailDto> { callDetailDto });
            var fileName = $"CallDetails_{call.DetailID}_{DateTime.Now:yyyyMMddHHmmss}.csv";

            return File(csv, "text/csv", fileName);
        }
        [HttpPost]
        public async Task<IActionResult> ExportWithOptions(CallSearchViewModel model, int limit = 1000, string columns = "")
        {
            var callFilterDto = new CallFilterDto
            {
                ANumber = model.Filter.ANumber,
                BNumber = model.Filter.BNumber,
                Answer = model.Filter.Answer,
                StartDate = model.Filter.StartDate,
                EndDate = model.Filter.EndDate,
                OriginCountryID = model.Filter.OriginCountryID,
                OriginCityID = model.Filter.OriginCityID,
                DestCountryID = model.Filter.DestCountryID,
                DestCityID = model.Filter.DestCityID,
                OriginOperatorID = model.Filter.OriginOperatorID,
                DestOperatorID = model.Filter.DestOperatorID,
                // استفاده از محدودیت تعداد رکوردها
                Page = 1,
                PageSize = limit
            };

            var data = await _callDetailRepository.GetFilteredAsync(callFilterDto);

            // لیست ستون‌های انتخاب شده
            var selectedColumns = columns.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();

            // ایجاد لیست DTO با ستون‌های انتخاب شده
            var callDetailDtos = data.Select(cd => {
                var dto = new CallDetailDto();

                if (selectedColumns.Contains("DetailID") || selectedColumns.Count == 0)
                    dto.DetailID = cd.DetailID;

                if (selectedColumns.Contains("ANumber") || selectedColumns.Count == 0)
                    dto.ANumber = cd.ANumber;

                if (selectedColumns.Contains("BNumber") || selectedColumns.Count == 0)
                    dto.BNumber = cd.BNumber;

                if (selectedColumns.Contains("AccountingTime") || selectedColumns.Count == 0)
                    dto.AccountingTime = cd.AccountingTime;

                if (selectedColumns.Contains("Length") || selectedColumns.Count == 0)
                    dto.Length = cd.Length;

                if (selectedColumns.Contains("OriginCountryName") || selectedColumns.Count == 0)
                    dto.OriginCountryName = cd.OriginCountry?.CountryName;

                if (selectedColumns.Contains("OriginCityName") || selectedColumns.Count == 0)
                    dto.OriginCityName = cd.OriginCity?.CityName;

                if (selectedColumns.Contains("OriginOperatorName") || selectedColumns.Count == 0)
                    dto.OriginOperatorName = cd.OriginOperator?.OperatorName;

                if (selectedColumns.Contains("DestCountryName") || selectedColumns.Count == 0)
                    dto.DestCountryName = cd.DestCountry?.CountryName;

                if (selectedColumns.Contains("DestCityName") || selectedColumns.Count == 0)
                    dto.DestCityName = cd.DestCity?.CityName;

                if (selectedColumns.Contains("DestOperatorName") || selectedColumns.Count == 0)
                    dto.DestOperatorName = cd.DestOperator?.OperatorName;

                if (selectedColumns.Contains("Answer") || selectedColumns.Count == 0)
                    dto.Answer = cd.Answer;

                return dto;
            }).ToList();

            var csv = ExportHelper.GenerateCsv(callDetailDtos, selectedColumns);
            var fileName = $"CallExport_{DateTime.Now:yyyyMMddHHmmss}.csv";

            return File(csv, "text/csv", fileName);
        }
    }
}
