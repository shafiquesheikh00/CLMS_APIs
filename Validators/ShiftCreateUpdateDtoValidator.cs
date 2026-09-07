using CLMS_APIs.Models.DTOs;
using FluentValidation;

namespace CLMS_APIs.Validators;

public class ShiftCreateUpdateDtoValidator : AbstractValidator<ShiftCreateUpdateDto>
{
    public ShiftCreateUpdateDtoValidator()
    {
        RuleFor(x => x.ShiftName)
            .NotEmpty().WithMessage("Shift name is required.")
            .MaximumLength(50).WithMessage("Shift name cannot exceed 50 characters.");

        RuleFor(x => x.StartTime)
            .NotEmpty().WithMessage("Start time is required.")
            .Must(BeValidTime).WithMessage("Start time must be a valid time (e.g. '09:00' or '22:00').");

        RuleFor(x => x.EndTime)
            .NotEmpty().WithMessage("End time is required.")
            .Must(BeValidTime).WithMessage("End time must be a valid time (e.g. '17:30' or '06:00').");

        RuleFor(x => x.GraceTime)
            .GreaterThanOrEqualTo(0).WithMessage("Grace time must be greater than or equal to 0.");

        RuleFor(x => x)
            .Custom((dto, context) =>
            {
                if (BeValidTime(dto.StartTime) && BeValidTime(dto.EndTime))
                {
                    var start = ParseTime(dto.StartTime);
                    var end = ParseTime(dto.EndTime);

                    if (!dto.IsOvernight && end <= start)
                    {
                        context.AddFailure(nameof(dto.EndTime), "End time must be after start time for a same-day shift.");
                    }
                }
            });
    }

    private static bool BeValidTime(string? timeStr)
    {
        if (string.IsNullOrWhiteSpace(timeStr))
            return false;

        return TimeOnly.TryParse(timeStr, out _) ||
               DateTime.TryParse(timeStr, out _) ||
               TimeSpan.TryParse(timeStr, out _);
    }

    private static TimeOnly ParseTime(string timeStr)
    {
        if (TimeOnly.TryParse(timeStr, out var time))
            return time;
        if (DateTime.TryParse(timeStr, out var dt))
            return TimeOnly.FromDateTime(dt);
        if (TimeSpan.TryParse(timeStr, out var ts))
            return TimeOnly.FromTimeSpan(ts);
        return TimeOnly.MinValue;
    }
}
