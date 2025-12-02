using AnalysisCallUser._01_Domain.Core.DTOs;
using AnalysisCallUser._01_Domain.Core.Enums;
using FluentValidation;

namespace AnalysisCallUser._03_EndPoint.Validators
{
    public class ExportValidator : AbstractValidator<ExportRequestDto>
    {
        public ExportValidator()
        {
            RuleFor(x => x.ExportType)
                .IsInEnum().WithMessage("Invalid export type specified.")
                .Must(BeAValidExportType).WithMessage("Selected export type is not supported.");

            RuleFor(x => x.TotalRecords)
                .GreaterThan(0).WithMessage("At least one record must be selected for export.")
                .LessThanOrEqualTo(100000).WithMessage("Cannot export more than 100,000 records at once.");

            RuleFor(x => x.Filter)
                .NotNull().WithMessage("Filter criteria are required for export.")
                .SetValidator(new FilterValidator()); // استفاده از Validator دیگر
        }

        private bool BeAValidExportType(ExportType exportType)
        {
            return Enum.IsDefined(typeof(ExportType), exportType) &&
                   exportType != ExportType.Excel; // مثلاً اگر اکسل هنوز پیاده‌سازی نشده
        }
    }
}
