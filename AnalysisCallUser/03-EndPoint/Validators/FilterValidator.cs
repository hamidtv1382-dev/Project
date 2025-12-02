using AnalysisCallUser._01_Domain.Core.DTOs;
using FluentValidation;

namespace AnalysisCallUser._03_EndPoint.Validators
{
    public class FilterValidator : AbstractValidator<CallFilterDto>
    {
        public FilterValidator()
        {
            RuleFor(x => x.StartDate)
                .LessThanOrEqualTo(x => x.EndDate)
                .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
                .WithMessage("Start date must be before or equal to the end date.");

            RuleFor(x => x.StartTime)
                .LessThanOrEqualTo(x => x.EndTime)
                .When(x => x.StartTime.HasValue && x.EndTime.HasValue)
                .WithMessage("Start time must be before or equal to the end time.");

            RuleFor(x => x.PageSize)
                .GreaterThan(0).WithMessage("Page size must be greater than zero.")
                .LessThanOrEqualTo(1000).WithMessage("Page size cannot exceed 1000.");

            RuleFor(x => x.Page)
                .GreaterThan(0).WithMessage("Page number must be greater than zero.");
        }
    }
}
