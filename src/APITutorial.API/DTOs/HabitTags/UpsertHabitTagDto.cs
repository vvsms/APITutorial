﻿namespace APITutorial.API.DTOs.HabitTags;

public sealed record UpsertHabitTagDto
{
    public required List<string> TagIds { get; init; }
}
