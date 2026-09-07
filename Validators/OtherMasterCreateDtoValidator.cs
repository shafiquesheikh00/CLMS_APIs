using CLMS_APIs.Models.DTOs;
using FluentValidation;

namespace CLMS_APIs.Validators;

public class OtherMasterCreateDtoValidator : AbstractValidator<OtherMasterCreateDto>
{
    public OtherMasterCreateDtoValidator()
    {
        RuleFor(x => x.MasterType)
            .NotEmpty().WithMessage("MasterType is required.")
            .MaximumLength(150).WithMessage("MasterType cannot exceed 150 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(200).WithMessage("Description cannot exceed 200 characters.");

        RuleFor(x => x)
            .Custom((dto, context) =>
            {
                var hasMasterId = dto.MasterId.HasValue && dto.MasterId.Value > 0;
                var hasNewCategory = !string.IsNullOrWhiteSpace(dto.NewMasterName);

                if (hasMasterId && hasNewCategory)
                {
                    context.AddFailure(nameof(dto.NewMasterName), "Provide either an existing MasterId or a NewMasterName, not both.");
                }
                else if (!hasMasterId && !hasNewCategory)
                {
                    context.AddFailure(nameof(dto.MasterId), "Either select an existing category (MasterId) or provide a NewMasterName for a new category.");
                }
                else if (hasNewCategory && dto.NewMasterName!.Trim().Length > 50)
                {
                    context.AddFailure(nameof(dto.NewMasterName), "NewMasterName cannot exceed 50 characters.");
                }
            });
    }
}
