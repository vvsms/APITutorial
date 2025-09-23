using APITutorial.API.Entities;
using Microsoft.AspNetCore.Mvc;

namespace APITutorial.API.DTOs.Habits;

public sealed record HabitsQueryParameters
{
    [FromQuery(Name = "q")]
    public string? Search { get; init; }

    public HabitType? Type { get; init; }

    public HabitStatus? Status { get; init; }

    public string? Sort { get; init; }
}