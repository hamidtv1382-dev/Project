using AnalysisCallUser._01_Domain.Core.Contracts;
using AnalysisCallUser._01_Domain.Core.DTOs;
using AnalysisCallUser._01_Domain.Services;
using AnalysisCallUser._02_Infrastructure.Data;
using AnalysisCallUser._02_Infrastructure.Helpers;
using AnalysisCallUser._03_EndPoint.Models.ViewModels.Call;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

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
            DateTime? startDateGregorian = null;
            DateTime? endDateGregorian = null;

            if (!string.IsNullOrEmpty(model.Filter.StartDate))
            {
                try
                {
                    startDateGregorian = PersianDateHelper.ToGregorian(model.Filter.StartDate);
                    if (!startDateGregorian.HasValue)
                        ModelState.AddModelError("Filter.StartDate", "تاریخ شروع نامعتبر است.");
                }
                catch
                {
                    ModelState.AddModelError("Filter.StartDate", "تاریخ شروع نامعتبر است.");
                }
            }

            if (!string.IsNullOrEmpty(model.Filter.EndDate))
            {
                try
                {
                    endDateGregorian = PersianDateHelper.ToGregorian(model.Filter.EndDate);
                    if (!endDateGregorian.HasValue)
                        ModelState.AddModelError("Filter.EndDate", "تاریخ پایان نامعتبر است.");
                }
                catch
                {
                    ModelState.AddModelError("Filter.EndDate", "تاریخ پایان نامعتبر است.");
                }
            }

            // فقط فیلترهای داخل model.Filter را چک می‌کنیم
            if (!ModelState.IsValid)
            {
                // در صورت بروز خطا در اعتبارسنجی، فقط خطا را برمی‌گردانیم
                var errors = ModelState.Where(x => x.Value.Errors.Count > 0)
                                      .Select(x => new { x.Key, x.Value.Errors })
                                      .ToList();
                return Json(new { success = false, message = "ModelState is invalid.", errors = errors });
            }

            var callFilterDto = new CallFilterDto
            {
                ANumber = model.Filter.ANumber,
                BNumber = model.Filter.BNumber,
                Answer = model.Filter.Answer,
                StartDate = startDateGregorian,
                EndDate = endDateGregorian,
                Page = model.Filter.Page,
                PageSize = model.Filter.PageSize,
                OriginCountryID = model.Filter.OriginCountryID,
                OriginCityID = model.Filter.OriginCityID,
                DestCountryID = model.Filter.DestCountryID,
                DestCityID = model.Filter.DestCityID,
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
                OriginOperatorName = cd.OriginOperator?.OperatorName,
                DestOperatorName = cd.DestOperator?.OperatorName,
                Answer = cd.Answer
            }).ToList();

            model.Results = new PagedResult<CallDetailDto>(callDetailDtos, count, model.Filter.Page, model.Filter.PageSize);

            // این داده‌ها برای رندر کردن کامل صفحه در درخواست‌های غیر-Ajax لازم است
            model.Countries = await _context.Countries.OrderBy(c => c.CountryName).ToListAsync();
            if (model.Filter.OriginCountryID.HasValue)
            {
                model.OriginCities = await _context.Cities.Where(c => c.CountryID == model.Filter.OriginCountryID.Value).ToListAsync();
                model.OriginOperators = await _context.Operators.Where(o => o.CountryID == model.Filter.OriginCountryID.Value).ToListAsync();
            }
            if (model.Filter.DestCountryID.HasValue)
            {
                model.DestCities = await _context.Cities.Where(c => c.CountryID == model.Filter.DestCountryID.Value).ToListAsync();
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
                OriginOperatorName = call.OriginOperator?.OperatorName,
                DestOperatorName = call.DestOperator?.OperatorName,
                Answer = call.Answer
            });

            return View(viewModel);
        }

        [HttpGet]
        public async Task<JsonResult> GetOperators(int countryId)
        {
            var operators = await _context.Operators
                                       .Where(o => o.CountryID == countryId)
                                       .OrderBy(o => o.OperatorName)
                                       .ToListAsync();
            return Json(operators);
        }

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

            return Json(new
            {
                success = (country != null),
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
            DateTime? startDateGregorian = null;
            DateTime? endDateGregorian = null;

            if (!string.IsNullOrEmpty(model.Filter.StartDate))
            {
                startDateGregorian = PersianDateHelper.ToGregorian(model.Filter.StartDate);
            }

            if (!string.IsNullOrEmpty(model.Filter.EndDate))
            {
                endDateGregorian = PersianDateHelper.ToGregorian(model.Filter.EndDate);
            }

            var callFilterDto = new CallFilterDto
            {
                ANumber = model.Filter.ANumber,
                BNumber = model.Filter.BNumber,
                Answer = model.Filter.Answer,
                StartDate = startDateGregorian,
                EndDate = endDateGregorian,
                OriginCountryID = model.Filter.OriginCountryID,
                OriginCityID = model.Filter.OriginCityID,
                DestCountryID = model.Filter.DestCountryID,
                DestCityID = model.Filter.DestCityID,
                OriginOperatorID = model.Filter.OriginOperatorID,
                DestOperatorID = model.Filter.DestOperatorID,
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

            // دریافت بایت‌های CSV از helper
            byte[] csvBytes = ExportHelper.GenerateCsv(callDetailDtos);
            var fileName = $"CallSearchResults_{DateTime.Now:yyyyMMddHHmmss}.csv";

            // اضافه کردن UTF-8 BOM در صورتی که موجود نباشد
            var utf8Bom = new byte[] { 0xEF, 0xBB, 0xBF };
            if (!(csvBytes.Length >= 3 && csvBytes[0] == utf8Bom[0] && csvBytes[1] == utf8Bom[1] && csvBytes[2] == utf8Bom[2]))
            {
                var withBom = new byte[csvBytes.Length + 3];
                Buffer.BlockCopy(utf8Bom, 0, withBom, 0, 3);
                Buffer.BlockCopy(csvBytes, 0, withBom, 3, csvBytes.Length);
                csvBytes = withBom;
            }

            return File(csvBytes, "text/csv; charset=utf-8", fileName);
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

            byte[] csvBytes = ExportHelper.GenerateCsv(new List<CallDetailDto> { callDetailDto });
            var fileName = $"CallDetails_{call.DetailID}_{DateTime.Now:yyyyMMddHHmmss}.csv";

            // اضافه کردن UTF-8 BOM در صورتی که موجود نباشد
            var utf8Bom = new byte[] { 0xEF, 0xBB, 0xBF };
            if (!(csvBytes.Length >= 3 && csvBytes[0] == utf8Bom[0] && csvBytes[1] == utf8Bom[1] && csvBytes[2] == utf8Bom[2]))
            {
                var withBom = new byte[csvBytes.Length + 3];
                Buffer.BlockCopy(utf8Bom, 0, withBom, 0, 3);
                Buffer.BlockCopy(csvBytes, 0, withBom, 3, csvBytes.Length);
                csvBytes = withBom;
            }

            return File(csvBytes, "text/csv; charset=utf-8", fileName);
        }

        [HttpPost]
        public async Task<IActionResult> ExportWithOptions(CallSearchViewModel model, int limit = 1000, string columns = "")
        {
            DateTime? startDateGregorian = null;
            DateTime? endDateGregorian = null;

            if (!string.IsNullOrEmpty(model.Filter.StartDate))
            {
                startDateGregorian = PersianDateHelper.ToGregorian(model.Filter.StartDate);
            }

            if (!string.IsNullOrEmpty(model.Filter.EndDate))
            {
                endDateGregorian = PersianDateHelper.ToGregorian(model.Filter.EndDate);
            }

            var callFilterDto = new CallFilterDto
            {
                ANumber = model.Filter.ANumber,
                BNumber = model.Filter.BNumber,
                Answer = model.Filter.Answer,
                StartDate = startDateGregorian,
                EndDate = endDateGregorian,
                OriginCountryID = model.Filter.OriginCountryID,
                OriginCityID = model.Filter.OriginCityID,
                DestCountryID = model.Filter.DestCountryID,
                DestCityID = model.Filter.DestCityID,
                OriginOperatorID = model.Filter.OriginOperatorID,
                DestOperatorID = model.Filter.DestOperatorID,
                Page = 1,
                PageSize = limit
            };

            var data = await _callDetailRepository.GetFilteredAsync(callFilterDto);

            var selectedColumns = columns.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();

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

            byte[] csvBytes = ExportHelper.GenerateCsv(callDetailDtos, selectedColumns);
            var fileName = $"CallExport_{DateTime.Now:yyyyMMddHHmmss}.csv";

            // اضافه کردن UTF-8 BOM در صورتی که موجود نباشد
            var utf8Bom = new byte[] { 0xEF, 0xBB, 0xBF };
            if (!(csvBytes.Length >= 3 && csvBytes[0] == utf8Bom[0] && csvBytes[1] == utf8Bom[1] && csvBytes[2] == utf8Bom[2]))
            {
                var withBom = new byte[csvBytes.Length + 3];
                Buffer.BlockCopy(utf8Bom, 0, withBom, 0, 3);
                Buffer.BlockCopy(csvBytes, 0, withBom, 3, csvBytes.Length);
                csvBytes = withBom;
            }

            return File(csvBytes, "text/csv; charset=utf-8", fileName);
        }
    }
}
