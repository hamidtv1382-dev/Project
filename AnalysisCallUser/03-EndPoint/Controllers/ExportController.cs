using AnalysisCallUser._01_Domain.Core.Contracts;
using AnalysisCallUser._01_Domain.Core.DTOs;
using AnalysisCallUser._01_Domain.Core.Enums;
using AnalysisCallUser._03_EndPoint.Models.ViewModels.Export;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AnalysisCallUser._03_EndPoint.Controllers
{
    [Authorize]
    public class ExportController : Controller
    {
        private readonly IExportService _exportService;

        public ExportController(IExportService exportService)
        {
            _exportService = exportService;
        }

        public IActionResult History()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RequestExport(ExportRequestViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);

                var exportRequestDto = new ExportRequestDto
                {
                    Filter = new CallFilterDto
                    {
                        StartDate = model.Filter.StartDate,
                        EndDate = model.Filter.EndDate,
                        StartTime = model.Filter.StartTime,
                        EndTime = model.Filter.EndTime,
                        ANumber = model.Filter.ANumber,
                        BNumber = model.Filter.BNumber,
                        OriginCountryID = model.Filter.OriginCountryID,
                        DestCountryID = model.Filter.DestCountryID,
                        OriginCityID = model.Filter.OriginCityID,
                        DestCityID = model.Filter.DestCityID,
                        OriginOperatorID = model.Filter.OriginOperatorID,
                        DestOperatorID = model.Filter.DestOperatorID,
                        TypeID = model.Filter.TypeID,
                        Answer = model.Filter.Answer.HasValue ? (CallAnswerStatus?)model.Filter.Answer.Value : null,
                        Page = model.Filter.Page,
                        PageSize = model.Filter.PageSize
                    },
                    ExportType = model.ExportType,
                    TotalRecords = model.TotalRecords
                };

                await _exportService.CreateExportRequestAsync(exportRequestDto, userId);
                return RedirectToAction("History");
            }
            return View(model);
        }
    }
}