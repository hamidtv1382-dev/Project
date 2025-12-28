using AnalysisCallUser._01_Domain.Core.Contracts;
using AnalysisCallUser._01_Domain.Core.DTOs;
using AnalysisCallUser._01_Domain.Core.Entities;
using AnalysisCallUser._01_Domain.Core.Enums;
using AnalysisCallUser._01_Domain.Services;
using AnalysisCallUser._02_Infrastructure.Data;
using AnalysisCallUser._02_Infrastructure.Helpers;
using AnalysisCallUser._03_EndPoint.Models.ViewModels.Call;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AnalysisCallUser._03_EndPoint.Controllers
{
    [Authorize]
    public class CallController : Controller
    {
        private readonly ICallDetailRepository _callDetailRepository;
        private readonly AppDbContext _context;
        private readonly IPhoneInfoService _phoneInfoService;

        private const string SessionFilterKey = "CallSearchFilters";

        public CallController(ICallDetailRepository callDetailRepository, AppDbContext context, IPhoneInfoService phoneInfoService)
        {
            _callDetailRepository = callDetailRepository;
            _context = context;
            _phoneInfoService = phoneInfoService;
        }

        #region Helper Methods

        private (DateTime? startDate, DateTime? endDate) ConvertPersianDates(string startDateStr, string endDateStr)
        {
            DateTime? startDate = null;
            DateTime? endDate = null;

            if (!string.IsNullOrEmpty(startDateStr))
            {
                try { startDate = PersianDateHelper.ToGregorian(startDateStr); }
                catch { ModelState.AddModelError("Filter.StartDate", "تاریخ شروع نامعتبر است."); }
            }

            if (!string.IsNullOrEmpty(endDateStr))
            {
                try { endDate = PersianDateHelper.ToGregorian(endDateStr); }
                catch { ModelState.AddModelError("Filter.EndDate", "تاریخ پایان نامعتبر است."); }
            }

            return (startDate, endDate);
        }

        private void PopulateModelFromSession(CallSearchViewModel model)
        {
            var filterJson = HttpContext.Session.GetString(SessionFilterKey);
            if (!string.IsNullOrEmpty(filterJson))
            {
                try
                {
                    var tempFilter = JsonSerializer.Deserialize<CallFilterViewModel>(filterJson);
                    if (tempFilter != null) model.Filter = tempFilter;
                }
                catch (JsonException)
                {
                    // اگر خطا در deserialize بود، session را پاک کن
                    HttpContext.Session.Remove(SessionFilterKey);
                }
            }
        }

        private void SaveModelToSession(CallSearchViewModel model)
        {
            try
            {
                var filterJson = JsonSerializer.Serialize(model.Filter);
                HttpContext.Session.SetString(SessionFilterKey, filterJson);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving to session: {ex.Message}");
            }
        }

        private void ClearSessionFilter()
        {
            HttpContext.Session.Remove(SessionFilterKey);
        }

        private void LogExport(string userName, IEnumerable<CallDetailDto> exportedRecords)
        {
            try
            {
                if (exportedRecords == null || !exportedRecords.Any()) return;

                int minId = exportedRecords.Min(r => r.DetailID);
                int maxId = exportedRecords.Max(r => r.DetailID);
                int count = exportedRecords.Count();
                string logLine = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}\tUser: {userName}\tRecords: {minId}-{maxId}\tCount: {count}";

                string logFilePath = Path.Combine(Directory.GetCurrentDirectory(), "ExportsLog.txt");
                System.IO.File.AppendAllText(logFilePath, logLine + Environment.NewLine);
            }
            catch
            {
                // اگر خطا رخ داد، نادیده گرفته شود
            }
        }

        private List<string> ExtractNumbersFromText(string numbersText)
        {
            var numbers = new List<string>();

            if (string.IsNullOrWhiteSpace(numbersText))
                return numbers;

            var lines = numbersText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (!string.IsNullOrWhiteSpace(trimmedLine))
                {
                    // جدا کردن با کاما، فاصله، یا tab
                    var parts = trimmedLine.Split(new[] { ',', ' ', '\t', ';' }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (var part in parts)
                    {
                        var number = part.Trim();
                        if (!string.IsNullOrWhiteSpace(number) && !numbers.Contains(number))
                        {
                            numbers.Add(number);
                        }
                    }
                }
            }

            return numbers;
        }

        private string FormatTime(int seconds)
        {
            if (seconds < 60) return $"{seconds} ثانیه";
            if (seconds < 3600) return $"{seconds / 60} دقیقه و {seconds % 60} ثانیه";
            return $"{seconds / 3600} ساعت و {(seconds % 3600) / 60} دقیقه";
        }

        private void LogWeightedExport(string userName, int resultCount)
        {
            try
            {
                string logLine = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}\tUser: {userName}\tWeightedResults: {resultCount}\tType: WeightedAnalysis";
                string logFilePath = Path.Combine(Directory.GetCurrentDirectory(), "WeightedExportsLog.txt");
                System.IO.File.AppendAllText(logFilePath, logLine + Environment.NewLine);
            }
            catch
            {
                // اگر خطا رخ داد، نادیده گرفته شود
            }
        }

        #endregion

        #region Session Management APIs

        [HttpPost]
        public IActionResult ClearSessionFilters()
        {
            ClearSessionFilter();
            return Json(new { success = true, message = "Session filters cleared successfully." });
        }

        [HttpGet]
        public IActionResult GetSessionFilters()
        {
            var filterJson = HttpContext.Session.GetString(SessionFilterKey);
            if (string.IsNullOrEmpty(filterJson))
            {
                return Json(new { success = false, message = "No filters found in session." });
            }

            try
            {
                var filter = JsonSerializer.Deserialize<CallFilterViewModel>(filterJson);
                return Json(new { success = true, filters = filter });
            }
            catch (JsonException ex)
            {
                ClearSessionFilter();
                return Json(new { success = false, message = $"Error deserializing filters: {ex.Message}" });
            }
        }

        #endregion

        [HttpGet]
        public async Task<IActionResult> Search()
        {
            var model = new CallSearchViewModel
            {
                Filter = new CallFilterViewModel { Page = 1, PageSize = 50 },
                Countries = await _context.Countries.OrderBy(c => c.CountryName).ToListAsync()
            };

            // بارگذاری فیلترها از Session
            PopulateModelFromSession(model);

            // اگر فیلتری در Session بود، نتایج را بارگذاری کن
            if (model.Filter != null && model.Filter.HasFilters())
            {
                await LoadSearchResults(model);
            }

            return View(model);
        }

        private bool HasFilters(CallFilterViewModel filter)
        {
            return filter != null && (
                !string.IsNullOrEmpty(filter.StartDate) ||
                !string.IsNullOrEmpty(filter.EndDate) ||
                filter.ANumbers?.Any(n => !string.IsNullOrWhiteSpace(n)) == true ||
                filter.BNumbers?.Any(n => !string.IsNullOrWhiteSpace(n)) == true ||
                filter.OriginCountryID.HasValue ||
                filter.DestCountryID.HasValue ||
                filter.OriginCityID.HasValue ||
                filter.DestCityID.HasValue ||
                filter.OriginOperatorID.HasValue ||
                filter.DestOperatorID.HasValue ||
                filter.Answer.HasValue);
        }

        private async Task LoadSearchResults(CallSearchViewModel model)
        {
            var (startDateGregorian, endDateGregorian) =
                ConvertPersianDates(model.Filter.StartDate, model.Filter.EndDate);

            var callFilterDto = new CallFilterDto
            {
                ANumbers = model.Filter.ANumbers,
                BNumbers = model.Filter.BNumbers,
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
                OriginOperatorName = cd.OriginOperator?.OperatorName,
                DestCountryName = cd.DestCountry?.CountryName,
                DestCityName = cd.DestCity?.CityName,
                DestOperatorName = cd.DestOperator?.OperatorName,
                Answer = cd.Answer
            }).ToList();

            model.Results = new PagedResult<CallDetailDto>(callDetailDtos, count, model.Filter.Page, model.Filter.PageSize);
        }

        [HttpPost]
        public async Task<IActionResult> Search(CallSearchViewModel model, IFormCollection form, bool deepSearch = false)
        {
            var originalTimeout = _context.Database.GetCommandTimeout();
            _context.Database.SetCommandTimeout(30);

            try
            {
                var (startDateGregorian, endDateGregorian) = ConvertPersianDates(model.Filter.StartDate, model.Filter.EndDate);

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Where(x => x.Value.Errors.Count > 0).Select(x => new { x.Key, x.Value.Errors }).ToList();
                    return Json(new { success = false, message = "ModelState is invalid.", errors = errors });
                }

                model.Filter.ANumbers = form["Filter.ANumbers"].ToList();
                model.Filter.BNumbers = form["Filter.BNumbers"].ToList();

                var callFilterDto = new CallFilterDto
                {
                    ANumbers = model.Filter.ANumbers,
                    BNumbers = model.Filter.BNumbers,
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
                    DestOperatorID = model.Filter.DestOperatorID,
                    IsDeepSearch = deepSearch,
                    BidirectionalSearch = true
                };

                bool skipCount = (startDateGregorian.HasValue && endDateGregorian.HasValue && (endDateGregorian.Value - startDateGregorian.Value).TotalDays > 90);

                int count = 0;
                if (!skipCount)
                {
                    try { count = await _callDetailRepository.GetFilteredCountAsync(callFilterDto); }
                    catch { skipCount = true; }
                }

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
                    OriginOperatorName = cd.OriginOperator?.OperatorName,
                    DestCountryName = cd.DestCountry?.CountryName,
                    DestCityName = cd.DestCity?.CityName,
                    DestOperatorName = cd.DestOperator?.OperatorName,
                    Answer = cd.Answer
                }).ToList();

                if (skipCount)
                {
                    count = callDetailDtos.Count >= model.Filter.PageSize ? (model.Filter.Page * model.Filter.PageSize) + 1 : ((model.Filter.Page - 1) * model.Filter.PageSize) + callDetailDtos.Count;
                }

                model.Results = new PagedResult<CallDetailDto>(callDetailDtos, count, model.Filter.Page, model.Filter.PageSize);

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
                    // ذخیره در Session
                    SaveModelToSession(model);
                    return PartialView("_SearchResults", model.Results);
                }

                // ذخیره در Session
                SaveModelToSession(model);
                return RedirectToAction(nameof(Search), model.Filter);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Search error: {ex.Message}");
                ModelState.AddModelError("", "خطا در دریافت نتایج جستجو.");
                return View(model);
            }
            finally
            {
                _context.Database.SetCommandTimeout(originalTimeout);
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            var call = await _callDetailRepository.GetByIdAsync(id);
            if (call == null) return NotFound();

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
            var operators = await _context.Operators.Where(o => o.CountryID == countryId).OrderBy(o => o.OperatorName).ToListAsync();
            return Json(operators);
        }

        [HttpGet]
        public async Task<JsonResult> GetCountries()
        {
            var countries = await _context.Countries.OrderBy(c => c.CountryName).ToListAsync();
            return Json(countries);
        }

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
            var cities = await _context.Cities.Where(c => c.CountryID == countryId).OrderBy(c => c.CityName).ToListAsync();
            return Json(cities);
        }

        [HttpPost]
        public async Task<IActionResult> ExportSearchResults(CallSearchViewModel model, IFormCollection form)
        {
            var originalTimeout = _context.Database.GetCommandTimeout();
            _context.Database.SetCommandTimeout(120);

            try
            {
                var (startDateGregorian, endDateGregorian) = ConvertPersianDates(model.Filter.StartDate, model.Filter.EndDate);

                model.Filter.ANumbers = form["Filter.ANumbers"].ToList();
                model.Filter.BNumbers = form["Filter.BNumbers"].ToList();

                var callFilterDto = new CallFilterDto
                {
                    ANumbers = model.Filter.ANumbers,
                    BNumbers = model.Filter.BNumbers,
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
                    OriginOperatorName = cd.OriginOperator?.OperatorName,
                    DestCountryName = cd.DestCountry?.CountryName,
                    DestCityName = cd.DestCity?.CityName,
                    DestOperatorName = cd.DestOperator?.OperatorName,
                    Answer = cd.Answer
                }).ToList();

                // ثبت لاگ Export
                LogExport(User.Identity.Name, callDetailDtos);

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
            if (call == null) return NotFound();

            var callDetailDto = new CallDetailDto
            {
                DetailID = call.DetailID,
                ANumber = call.ANumber,
                BNumber = call.BNumber,
                AccountingTime = call.AccountingTime,
                Length = call.Length,
                OriginCountryName = call.OriginCountry?.CountryName,
                DestCountryName = call.DestCountry?.CountryName,
                OriginCityName = call.OriginCity?.CityName,
                DestCityName = call.DestCity?.CityName,
                OriginOperatorName = call.OriginOperator?.OperatorName,
                DestOperatorName = call.DestOperator?.OperatorName,
                Answer = call.Answer
            };

            LogExport(User.Identity.Name, new List<CallDetailDto> { callDetailDto });

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
            _context.Database.SetCommandTimeout(120);

            try
            {
                var (startDateGregorian, endDateGregorian) = ConvertPersianDates(model.Filter.StartDate, model.Filter.EndDate);

                model.Filter.ANumbers = form["Filter.ANumbers"].ToList();
                model.Filter.BNumbers = form["Filter.BNumbers"].ToList();

                var callFilterDto = new CallFilterDto
                {
                    ANumbers = model.Filter.ANumbers,
                    BNumbers = model.Filter.BNumbers,
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

                var callDetailDtos = data.Select(cd =>
                {
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

                LogExport(User.Identity.Name, callDetailDtos);

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

        [HttpPost]
        public async Task<IActionResult> ExportSelectedCalls([FromBody] ExportSelectedRequest request)
        {
            if (request?.SelectedCallIds == null || !request.SelectedCallIds.Any())
            {
                return BadRequest("هیچ موردی انتخاب نشده است");
            }

            var originalTimeout = _context.Database.GetCommandTimeout();
            _context.Database.SetCommandTimeout(120);

            try
            {
                // تبدیل رشته‌ها به عدد
                var callIds = request.SelectedCallIds
                    .Select(id => int.TryParse(id, out var num) ? num : (int?)null)
                    .Where(id => id.HasValue)
                    .Select(id => id.Value)
                    .ToList();

                if (!callIds.Any())
                {
                    return BadRequest("شناسه‌های انتخاب شده نامعتبر هستند");
                }

                // دریافت اطلاعات تماس‌های انتخاب شده
                var selectedCalls = await _callDetailRepository.GetByIdsAsync(callIds);

                var callDetailDtos = selectedCalls.Select(cd => new CallDetailDto
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

                // ثبت لاگ
                LogExport(User.Identity.Name, callDetailDtos);

                // تولید CSV
                byte[] csvBytes = ExportHelper.GenerateCsv(callDetailDtos);
                var fileName = $"SelectedCalls_{DateTime.Now:yyyyMMddHHmmss}.csv";

                // اضافه کردن BOM برای UTF-8
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
            catch (Exception ex)
            {
                return StatusCode(500, $"خطا در اکسپورت: {ex.Message}");
            }
            finally
            {
                _context.Database.SetCommandTimeout(originalTimeout);
            }
        }

        #region Weighted Search Methods

        [HttpGet]
        public IActionResult WeightedSearch()
        {
            var model = new WeightedSearchViewModel
            {
                Filter = new WeightedSearchFilterViewModel()
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> WeightedSearch(WeightedSearchViewModel model, IFormFile sourceNumbersFile, IFormFile destNumbersFile)
        {
            var originalTimeout = _context.Database.GetCommandTimeout();
            _context.Database.SetCommandTimeout(300);

            try
            {
                // دریافت شماره‌های مبدأ از فایل
                if (sourceNumbersFile != null && sourceNumbersFile.Length > 0)
                {
                    using var reader = new StreamReader(sourceNumbersFile.OpenReadStream());
                    var content = await reader.ReadToEndAsync();
                    model.Filter.SourceNumbersText = content;
                }

                // دریافت شماره‌های مقصد از فایل
                if (destNumbersFile != null && destNumbersFile.Length > 0)
                {
                    using var reader = new StreamReader(destNumbersFile.OpenReadStream());
                    var content = await reader.ReadToEndAsync();
                    model.Filter.DestNumbersText = content;
                }

                // --- اصلاح شده: استخراج شماره‌ها به صورت خط به خط (Split) ---
                var sourceNumbers = new List<string>();
                var destNumbers = new List<string>();

                if (!string.IsNullOrWhiteSpace(model.Filter.SourceNumbersText))
                {
                    var lines = model.Filter.SourceNumbersText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    sourceNumbers = lines.Select(l => l.Trim()).Where(l => !string.IsNullOrWhiteSpace(l)).Distinct().ToList();
                }

                if (!string.IsNullOrWhiteSpace(model.Filter.DestNumbersText))
                {
                    var lines = model.Filter.DestNumbersText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    destNumbers = lines.Select(l => l.Trim()).Where(l => !string.IsNullOrWhiteSpace(l)).Distinct().ToList();
                }
                // ----------------------------------------------

                // بررسی که حداقل یکی از فیلدها پر باشد
                if (!sourceNumbers.Any() && !destNumbers.Any())
                {
                    ModelState.AddModelError("", "لطفاً حداقل یک شماره در مبدأ یا مقصد وارد کنید.");
                    return View(model);
                }

                var (startDateGregorian, endDateGregorian) =
                    ConvertPersianDates(model.Filter.StartDate, model.Filter.EndDate);

                // تعیین حالت جستجو بر اساس ورودی کاربر
                WeightedSearchMode searchMode;

                if (sourceNumbers.Any() && destNumbers.Any())
                {
                    searchMode = WeightedSearchMode.SourceDestinationPairs;
                }
                else if (sourceNumbers.Any())
                {
                    searchMode = WeightedSearchMode.SourceOnly;
                }
                else
                {
                    searchMode = WeightedSearchMode.DestinationOnly;
                }

                // ایجاد DTO برای جستجوی وزنی
                var weightedSearchDto = new WeightedSearchDto
                {
                    ANumbers = sourceNumbers,
                    BNumbers = destNumbers,
                    StartDate = startDateGregorian,
                    EndDate = endDateGregorian,
                    MinWeight = model.Filter.MinWeight,
                    BidirectionalSearch = model.Filter.IncludeReversePairs,
                    SearchMode = searchMode,
                    IncludeAnsweredCallsOnly = model.Filter.IncludeAnsweredCallsOnly
                };

                // فراخوانی متد جدید ریپازیتوری
                var weightedResults = await _callDetailRepository.GetWeightedSearchAsync(weightedSearchDto);

                // تبدیل به ViewModel
                model.WeightedResults = weightedResults.Select(r => new WeightedCallResultViewModel
                {
                    ANumber = r.ANumber,
                    BNumber = r.BNumber,
                    Weight = r.Weight,
                    TotalLength = r.TotalLength,
                    AverageLength = r.AverageLength,
                    DirectCalls = r.DirectCalls,
                    ReverseCalls = r.ReverseCalls,
                    SearchType = r.SearchType,
                    DirectionInfo = r.DirectionInfo,
                    TotalLengthFormatted = FormatTime(r.TotalLength),
                    AverageLengthFormatted = FormatTime((int)r.AverageLength)
                }).ToList();

                model.TotalPairs = model.WeightedResults.Count;
                model.TotalCalls = model.WeightedResults.Sum(w => w.Weight);
                model.TotalLength = model.WeightedResults.Sum(w => w.TotalLength);

                // ذخیره در TempData برای اکسپورت
                TempData["WeightedResults"] = JsonSerializer.Serialize(model.WeightedResults);

                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"خطا در پردازش داده‌ها: {ex.Message}");
                Console.WriteLine($"WeightedSearch error: {ex}");
                return View(model);
            }
            finally
            {
                _context.Database.SetCommandTimeout(originalTimeout);
            }
        }

        [HttpPost]
        public async Task<IActionResult> ExportWeightedResults([FromBody] ExportWeightedRequest request)
        {
            try
            {
                List<WeightedCallResultViewModel> weightedResults;

                // استفاده از داده‌های ارسال شده از کلاینت
                if (request != null && request.WeightedResults != null && request.WeightedResults.Any())
                {
                    weightedResults = request.WeightedResults;
                }
                // در غیر این صورت از TempData بازیابی می‌کنیم
                else if (TempData["WeightedResults"] is string tempDataJson)
                {
                    weightedResults = JsonSerializer.Deserialize<List<WeightedCallResultViewModel>>(tempDataJson);
                    TempData.Keep("WeightedResults");
                }
                else
                {
                    return BadRequest("داده‌ای برای اکسپورت یافت نشد.");
                }

                if (!weightedResults.Any())
                {
                    return BadRequest("هیچ نتیجه‌ای برای اکسپورت وجود ندارد.");
                }

                // تولید CSV
                using var memoryStream = new MemoryStream();
                using var writer = new StreamWriter(memoryStream, System.Text.Encoding.UTF8);

                // هدرها
                writer.WriteLine("شماره مبدأ,شماره مقصد,تعداد تماس,طول کل مکالمه(ثانیه),میانگین طول مکالمه(ثانیه),تماس مستقیم,تماس معکوس,نوع جستجو,جهت تماس");

                // داده‌ها
                foreach (var result in weightedResults)
                {
                    writer.WriteLine($"\"{result.ANumber}\",\"{result.BNumber}\",{result.Weight},{result.TotalLength},{result.AverageLength:F2},{result.DirectCalls},{result.ReverseCalls},{result.SearchType},{result.DirectionInfo}");
                }

                writer.Flush();
                memoryStream.Position = 0;

                var csvBytes = memoryStream.ToArray();
                var fileName = $"WeightedCallAnalysis_{DateTime.Now:yyyyMMddHHmmss}.csv";

                // اضافه کردن BOM برای UTF-8
                var utf8Bom = new byte[] { 0xEF, 0xBB, 0xBF };
                var withBom = new byte[csvBytes.Length + 3];
                Buffer.BlockCopy(utf8Bom, 0, withBom, 0, 3);
                Buffer.BlockCopy(csvBytes, 0, withBom, 3, csvBytes.Length);

                // ثبت لاگ
                LogWeightedExport(User.Identity.Name, weightedResults.Count);

                return File(withBom, "text/csv; charset=utf-8", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"خطا در اکسپورت: {ex.Message}");
            }
        }

        #endregion

        // کلاس‌های کمکی برای جستجوی وزنی قدیمی (در صورت نیاز می‌توانید حذف کنید)
        private List<CallPair> ExtractCallPairs(string numbersText)
        {
            var callPairs = new List<CallPair>();

            if (string.IsNullOrWhiteSpace(numbersText))
                return callPairs;

            var lines = numbersText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (string.IsNullOrWhiteSpace(trimmedLine))
                    continue;

                // جدا کردن بر اساس کاما
                var parts = trimmedLine.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length >= 2)
                {
                    var aNumber = parts[0].Trim();
                    var bNumber = parts[1].Trim();

                    if (!string.IsNullOrWhiteSpace(aNumber) && !string.IsNullOrWhiteSpace(bNumber))
                    {
                        callPairs.Add(new CallPair
                        {
                            ANumber = aNumber,
                            BNumber = bNumber
                        });
                    }
                }
            }

            return callPairs;
        }

        private class CallPair
        {
            public string ANumber { get; set; }
            public string BNumber { get; set; }
        }
    }


    public class WeightedCallResult
    {
        public string ANumber { get; set; }
        public string BNumber { get; set; }
        public int Weight { get; set; }
        public int TotalLength { get; set; } // مجموع طول تمام مکالمات به ثانیه
        public int DirectCalls { get; set; } // تماس‌های مستقیم (A->B)
        public int ReverseCalls { get; set; } // تماس‌های معکوس (B->A)
        public bool IsSourceSearch { get; set; } // آیا جستجو بر اساس مبدأ بوده؟
        public double AverageLength => Weight > 0 ? (double)TotalLength / Weight : 0;

        // پراپرتی‌های کمکی
        public string SearchType => IsSourceSearch ? "جستجوی مبدأ" : "جستجوی مقصد";
        public string DirectionInfo
        {
            get
            {
                if (DirectCalls > 0 && ReverseCalls > 0)
                    return "دوطرفه";
                else if (DirectCalls > 0)
                    return "مستقیم";
                else if (ReverseCalls > 0)
                    return "معکوس";
                return "-";
            }
        }
    }


    // کلاس‌های درخواست
    public class ExportWeightedRequest
    {
        public List<WeightedCallResultViewModel> WeightedResults { get; set; }
    }

    public class ExportSelectedRequest
    {
        public List<string> SelectedCallIds { get; set; }
    }
}