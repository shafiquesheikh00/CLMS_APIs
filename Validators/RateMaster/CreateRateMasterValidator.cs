using CLMS_APIs.Models.DTOs.RateMaster;
using FluentValidation;

namespace CLMS_APIs.Validators.RateMaster;

public class CreateRateMasterValidator : AbstractValidator<CreateRateMasterRequest>
{
    public CreateRateMasterValidator()
    {
        RuleFor(x => x.LabourCatId)
            .NotNull().WithMessage("Category (LabourCatID) is required.")
            .GreaterThan(0).WithMessage("Category ID must be a valid positive number.");

        RuleFor(x => x.RdateFrom)
            .NotNull().WithMessage("From Date is required.");

        RuleFor(x => x.Rdateto)
            .NotNull().WithMessage("To Date is required.");

        RuleFor(x => x.EmpCategoryFlag)
            .NotEmpty().WithMessage("Category type (EmpCategoryFlag) is required.")
            .Must(type => type.Equals("Labour", StringComparison.OrdinalIgnoreCase) ||
                          type.Equals("Employee", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Category type must be either 'Labour' or 'Employee'.");

        RuleFor(x => x)
            .Custom((req, context) =>
            {
                if (req.RdateFrom.HasValue && req.Rdateto.HasValue)
                {
                    if (req.RdateFrom.Value >= req.Rdateto.Value)
                    {
                        context.AddFailure(nameof(req.RdateFrom), "From date must be earlier than To date.");
                    }
                }
            });

        RuleFor(x => x.Basic).GreaterThanOrEqualTo(0).When(x => x.Basic.HasValue).WithMessage("Basic cannot be negative.");
        RuleFor(x => x.SpecialAllowance).GreaterThanOrEqualTo(0).When(x => x.SpecialAllowance.HasValue).WithMessage("Special Allowance cannot be negative.");
        RuleFor(x => x.Hra).GreaterThanOrEqualTo(0).When(x => x.Hra.HasValue).WithMessage("HRA cannot be negative.");
        RuleFor(x => x.OtherAllowance).GreaterThanOrEqualTo(0).When(x => x.OtherAllowance.HasValue).WithMessage("Other Allowance cannot be negative.");
        RuleFor(x => x.Stipend).GreaterThanOrEqualTo(0).When(x => x.Stipend.HasValue).WithMessage("Stipend cannot be negative.");
        RuleFor(x => x.RateOTPerHour).GreaterThanOrEqualTo(0).When(x => x.RateOTPerHour.HasValue).WithMessage("OT Rate cannot be negative.");
    }
}
