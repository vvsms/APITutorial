using APITutorial.API.Entities;
using FluentValidation;

namespace APITutorial.API.DTOs.Habits;

public sealed class CreateHabitDtoValidator : AbstractValidator<CreateHabitDto>
{
    private static readonly string[] AllowedUnits = [
        "minutes","hours","steps","km","cal","pages","books","tasks","sessions"
        ];

    private static readonly string[] AllowedUnitsForBinaryHabits = ["sessions", "tasks"];
    public CreateHabitDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(100)
            .WithMessage("Habit must be between 3 and 100 character");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Description cannot exide 500 character");

        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Invalid habit type");

        RuleFor(x => x.Frequency.Type)
           .IsInEnum()
           .WithMessage("Invalid Frequency period");

        RuleFor(x => x.Frequency.TimesPerPeriod)
           .GreaterThan(0)
           .WithMessage("Frequency must be greater than 0");

        RuleFor(x => x.Target.Value)
           .GreaterThan(0)
           .WithMessage("Target must be greater than 0");

        RuleFor(x => x.Target.Unit)
           .NotEmpty()
           .Must(unit => AllowedUnits.Contains(unit.ToLowerInvariant()))
           .WithMessage($"Unit must be one of:{string.Join(", ", AllowedUnits)}");

        RuleFor(x => x.EndDate)
           .Must(date => date is null || date.Value > DateOnly.FromDateTime(DateTime.UtcNow))
           .WithMessage("End date must be in future");

        When(x => x.Milestone is not null, () =>
        {
            RuleFor(x => x.Milestone!.Target)
            .GreaterThan(0)
            .WithMessage("Milestone target must be greater than 0");
        });
        //complex rule
        RuleFor(x => x.Target.Unit)
            .Must((dto, unit) => IsTargetUnitcompatibleWithType(dto.Type, unit))
            .WithMessage("Target unit is not compatible With habit Type");
    }

    private static bool IsTargetUnitcompatibleWithType(HabitType type, string unit)
    {
        string normalizedUnit = unit.ToLowerInvariant();
        return type switch
        {
            HabitType.Binary => AllowedUnitsForBinaryHabits.Contains(normalizedUnit), //Binary habit should only use count-based unit
            HabitType.Measurable => AllowedUnits.Contains(normalizedUnit), // Mesurable habit can be use any of the allowed Unit
            _ => false //None is valid
        };
    }
}