public class CreateMaintenanceLogValidator : AbstractValidator<CreateLogRequest>
{
    public CreateMaintenanceLogValidator()
    {
        RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
        RuleFor(x => x.TechnicianName).NotEmpty();
        RuleFor(x => x.Cost).GreaterThanOrEqualTo(0)
            .WithMessage("Please enter a valid cost.");
    }
}