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
using System.Text.Json;
using NumberPairFilter = AnalysisCallUser._01_Domain.Core.DTOs.NumberPairFilter;

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

        #region Helper Methods

        /// <summary>
        /// Converts Persian date strings to Gregorian DateTime objects and adds model errors if conversion fails.
        /// </summary>
        /// <param name="startDateStr">The Persian start date string.</param>
        /// <param name="endDateStr">The Persian end date string.</param>
        /// <returns>A tuple containing the nullable start and end Gregorian dates.</returns>
        private (DateTime? startDate, DateTime? endDate) ConvertPersianDates(string startDateStr, string endDateStr)
        {
            DateTime? startDate = null;
            DateTime? endDate = null;

            if (!string.IsNullOrEmpty(startDateStr))
            {
                try
                {
                    startDate = PersianDateHelper.ToGregorian(startDateStr);
                }
                catch
                {
                    ModelState.AddModelError("Filter.StartDate", "تاریخ شروع نامعتبر است.");
                }
            }

            if (!string.IsNullOrEmpty(endDateStr))
            {
                try
                {
                    endDate = PersianDateHelper.ToGregorian(endDateStr);
                }
                catch
                {
                    ModelState.AddModelError("Filter.EndDate", "تاریخ پایان نامعتبر است.");
                }
            }
            return (startDate, endDate);
        }

        /// <summary>
        /// Parses number pairs from form data
        /// </summary>
        private List<NumberPairFilter> ParseNumberPairs(IFormCollection form)
        {
            var numberPairs = new List<NumberPairFilter>();

            // Get number pairs from form data
            var numberPairValues = form["Filter.NumberPairs"];
            foreach (var value in numberPairValues)
            {
                if (!string.IsNullOrEmpty(value))
                {
                    try
                    {
                        var pair = JsonSerializer.Deserialize<NumberPairFilter>(value);
                        if (pair != null)
                        {
                            numberPairs.Add(pair);
                        }
                    }
                    catch
                    {
                        // Skip invalid JSON
                        continue;
                    }
                }
            }

            return numberPairs;
        }

        #endregion

        // GET: /Call/Search
        [HttpGet]
        public async Task<IActionResult> Search()
        {
            var model = new CallSearchViewModel
            {
                Filter = new CallFilterViewModel { Page = 1, PageSize = 50 }, // Default values
                Countries = await _context.Countries.OrderBy(c => c.CountryName).ToListAsync()
            };
            return View(model);
        }

        // POST: /Call/Search
        [HttpPost]
        public async Task<IActionResult> Search(CallSearchViewModel model, IFormCollection form)
        {
            // Set a timeout for this operation to prevent hanging, especially on large datasets
            var originalTimeout = _context.Database.GetCommandTimeout();
            _context.Database.SetCommandTimeout(30); // 30 seconds timeout

            try
            {
                var (startDateGregorian, endDateGregorian) = ConvertPersianDates(model.Filter.StartDate, model.Filter.EndDate);

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Where(x => x.Value.Errors.Count > 0)
                                          .Select(x => new { x.Key, x.Value.Errors })
                                          .ToList();
                    return Json(new { success = false, message = "ModelState is invalid.", errors = errors });
                }

                // Parse ANumbers and BNumbers from form data
                var aNumbers = form["Filter.ANumbers"].ToList();
                var bNumbers = form["Filter.BNumbers"].ToList();

                // Parse NumberPairs from form data
                var numberPairs = ParseNumberPairs(form);

                var callFilterDto = new CallFilterDto
                {
                    ANumbers = aNumbers,
                    BNumbers = bNumbers,
                    NumberPairs = numberPairs,
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

                // NOTE: The previous check for 'isWideDateRange' has been removed.
                // The new index on the 'AccountingTime' column (IX_CallDetails_AccountingTime)
                // significantly accelerates date range queries, making searches over large time spans efficient.

                // This optimization remains valuable for extremely large datasets to avoid a potentially
                // expensive second query (the total count query).
                bool skipCount = (startDateGregorian.HasValue && endDateGregorian.HasValue &&
                                  (endDateGregorian.Value - startDateGregorian.Value).TotalDays > 90);

                int count = 0;
                if (!skipCount)
                {
                    try
                    {
                        count = await _callDetailRepository.GetFilteredCountAsync(callFilterDto);
                    }
                    catch (Exception ex)
                    {
                        // If the count query times out, we skip it and proceed to fetch just the data.
                        skipCount = true;
                        // In a real-world application, you would log this exception.
                        // _logger.LogError(ex, "Count query timed out during search. Skipping count.");
                    }
                }

                // Fetch the data for the current page. This query will now leverage the new indexes
                // (e.g., IX_CallDetails_AccountingTime, IX_CallDetails_ANumber, IX_CallDetails_Origin_Composite).
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

                // If we skipped the count, we provide an estimated count to enable pagination.
                if (skipCount)
                {
                    count = callDetailDtos.Count >= model.Filter.PageSize ?
                            (model.Filter.Page * model.Filter.PageSize) + 1 : // Indicates there are more pages
                            ((model.Filter.Page - 1) * model.Filter.PageSize) + callDetailDtos.Count;
                }

                model.Results = new PagedResult<CallDetailDto>(callDetailDtos, count, model.Filter.Page, model.Filter.PageSize);

                // This data is necessary for rendering the full page on non-Ajax requests
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
            catch (Exception ex)
            {
                // In a real-world application, you would log this exception.
                // _logger.LogError(ex, "An error occurred during the search operation.");

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new
                    {
                        success = false,
                        message = "خطا در دریافت نتایج جستجو. لطفاً فیلترهای خود را محدودتر کرده یا دوباره تلاش کنید."
                    });
                }

                ModelState.AddModelError("", "خطا در دریافت نتایج جستجو. لطفاً فیلترهای خود را محدودتر کرده یا دوباره تلاش کنید.");
                return View(model);
            }
            finally
            {
                // Restore the original timeout to not affect other operations
                _context.Database.SetCommandTimeout(originalTimeout);
            }
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
        // GET: /Call/GetPhoneInfo
        public async Task<JsonResult> GetPhoneInfo(string number)
        {
            var (country, city, op) = await _phoneInfoService.GetPhoneInfoAsync(number);

            return Json(new
            {
                success = (country != null),
                countryName = country?.CountryName,
                cityName = city?.CityName,
                operatorName = op?.OperatorName
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
        public async Task<IActionResult> ExportSearchResults(CallSearchViewModel model, IFormCollection form)
        {
            var originalTimeout = _context.Database.GetCommandTimeout();
            // Set a longer timeout for export operations as they can be time-consuming
            _context.Database.SetCommandTimeout(120); // 2 minutes timeout

            try
            {
                var (startDateGregorian, endDateGregorian) = ConvertPersianDates(model.Filter.StartDate, model.Filter.EndDate);

                // Parse ANumbers and BNumbers from form data
                var aNumbers = form["Filter.ANumbers"].ToList();
                var bNumbers = form["Filter.BNumbers"].ToList();

                // Parse NumberPairs from form data
                var numberPairs = ParseNumberPairs(form);

                var callFilterDto = new CallFilterDto
                {
                    ANumbers = aNumbers,
                    BNumbers = bNumbers,
                    NumberPairs = numberPairs,
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
                    PageSize = int.MaxValue // Export all results
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

                byte[] csvBytes = ExportHelper.GenerateCsv(callDetailDtos);
                var fileName = $"CallSearchResults_{DateTime.Now:yyyyMMddHHmmss}.csv";

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
            finally
            {
                _context.Database.SetCommandTimeout(originalTimeout);
            }
        }

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
        public async Task<IActionResult> ExportWithOptions(CallSearchViewModel model, IFormCollection form, int limit = 1000, string columns = "")
        {
            var originalTimeout = _context.Database.GetCommandTimeout();
            _context.Database.SetCommandTimeout(120); // 2 minutes timeout

            try
            {
                var (startDateGregorian, endDateGregorian) = ConvertPersianDates(model.Filter.StartDate, model.Filter.EndDate);

                // Parse ANumbers and BNumbers from form data
                var aNumbers = form["Filter.ANumbers"].ToList();
                var bNumbers = form["Filter.BNumbers"].ToList();

                // Parse NumberPairs from form data
                var numberPairs = ParseNumberPairs(form);

                var callFilterDto = new CallFilterDto
                {
                    ANumbers = aNumbers,
                    BNumbers = bNumbers,
                    NumberPairs = numberPairs,
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
                    if (selectedColumns.Contains("DetailID") || selectedColumns.Count == 0) dto.DetailID = cd.DetailID;
                    if (selectedColumns.Contains("ANumber") || selectedColumns.Count == 0) dto.ANumber = cd.ANumber;
                    if (selectedColumns.Contains("BNumber") || selectedColumns.Count == 0) dto.BNumber = cd.BNumber;
                    if (selectedColumns.Contains("AccountingTime") || selectedColumns.Count == 0) dto.AccountingTime = cd.AccountingTime;
                    if (selectedColumns.Contains("Length") || selectedColumns.Count == 0) dto.Length = cd.Length;
                    if (selectedColumns.Contains("OriginCountryName") || selectedColumns.Count == 0) dto.OriginCountryName = cd.OriginCountry?.CountryName;
                    if (selectedColumns.Contains("OriginCityName") || selectedColumns.Count == 0) dto.OriginCityName = cd.OriginCity?.CityName;
                    if (selectedColumns.Contains("OriginOperatorName") || selectedColumns.Count == 0) dto.OriginOperatorName = cd.OriginOperator?.OperatorName;
                    if (selectedColumns.Contains("DestCountryName") || selectedColumns.Count == 0) dto.DestCountryName = cd.DestCountry?.CountryName;
                    if (selectedColumns.Contains("DestCityName") || selectedColumns.Count == 0) dto.DestCityName = cd.DestCity?.CityName;
                    if (selectedColumns.Contains("DestOperatorName") || selectedColumns.Count == 0) dto.DestOperatorName = cd.DestOperator?.OperatorName;
                    if (selectedColumns.Contains("Answer") || selectedColumns.Count == 0) dto.Answer = cd.Answer;
                    return dto;
                }).ToList();

                byte[] csvBytes = ExportHelper.GenerateCsv(callDetailDtos, selectedColumns);
                var fileName = $"CallExport_{DateTime.Now:yyyyMMddHHmmss}.csv";

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
            finally
            {
                _context.Database.SetCommandTimeout(originalTimeout);
            }
        }
    }
}