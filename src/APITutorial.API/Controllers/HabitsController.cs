using System.Dynamic;
using APITutorial.API.Database;
using APITutorial.API.DTOs.Common;
using APITutorial.API.DTOs.Habits;
using APITutorial.API.Entities;
using APITutorial.API.Services;
using APITutorial.API.Services.Sorting;
using FluentValidation;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APITutorial.API.Controllers;


[ApiController]
[Route("api/[controller]")]
public sealed class HabitsController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetHabits(
        [FromQuery] HabitsQueryParameters queryParams,
        SortMappingProvider sortMappingProvider,
        DataShapingService dataShapingService,
        CancellationToken cancellationToken)
    {
        if (!sortMappingProvider.ValidateMappings<HabitDto, Habit>(queryParams.Sort))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: $"The sort parameter '{queryParams.Sort}' contains one or more invalid sort fields."
                );
        }

        if (!dataShapingService.Validate<HabitDto>(queryParams.Fields))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: $"The fields parameter '{queryParams.Fields}' contains one or more invalid fields."
                );
        }

        string? searchTerm = queryParams.Search?.Trim().ToLowerInvariant();

        SortMapping[] sortMappings = sortMappingProvider.GetMappings<HabitDto, Habit>();

        IQueryable<HabitDto> habitsQuery = dbContext.Habits.AsNoTracking()
            .Where(x =>
                searchTerm == null ||
                EF.Functions.ILike(x.Name, $"%{searchTerm}%") ||
                (x.Description != null && EF.Functions.ILike(x.Description, $"%{searchTerm}%")))
            .Where(x => queryParams.Type == null || x.Type == queryParams.Type)
            .Where(x => queryParams.Status == null || x.Status == queryParams.Status)
            .ApplySort(queryParams.Sort, sortMappings)
            .Select(HabitQueries.ProjectToDto());


        int totalCount = await habitsQuery.CountAsync(cancellationToken);

        List<HabitDto> habits = await habitsQuery
            .Skip((queryParams.Page - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .ToListAsync(cancellationToken);


        var paginationResult = new PaginationResult<ExpandoObject>
        {
            Items = dataShapingService.ShapeCollectionData(habits, queryParams.Fields),
            Page = queryParams.Page,
            PageSize = queryParams.PageSize,
            TotalCount = totalCount
        };

        return Ok(paginationResult);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetHabit(string id, string fields, DataShapingService dataShapingService, CancellationToken cancellationToken)
    {
        if (!dataShapingService.Validate<HabitWithTagsDto>(fields))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: $"The fields parameter '{fields}' contains one or more invalid fields."
                );
        }

        HabitWithTagsDto? habits = await dbContext.Habits
            .Where(h => h.Id == id)
            .Select(HabitQueries.ProjectToHabitWithTagsDto())
            .FirstOrDefaultAsync(cancellationToken);
        if (habits is null)
        {
            return NotFound();
        }
        ExpandoObject shapedHabit = dataShapingService.ShapeData(habits, fields);
        return Ok(shapedHabit);
    }

    [HttpPost]
    public async Task<ActionResult<HabitDto>> CreateHabit(CreateHabitDto createHabitDto, IValidator<CreateHabitDto> validator, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(createHabitDto, cancellationToken);

        var habit = createHabitDto.ToEntity();
        dbContext.Habits.Add(habit);

        await dbContext.SaveChangesAsync(cancellationToken);

        var habitDto = habit.ToDto();

        return CreatedAtAction(nameof(GetHabits), new { id = habit.Id }, habitDto);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<HabitDto>> UpdateHabit(string id, UpdateHabitDto updateHabitDto, CancellationToken cancellationToken)
    {
        Habit? habit = await dbContext.Habits.FirstOrDefaultAsync(h => h.Id == id, cancellationToken);

        if (habit == null)
        {
            return NotFound();
        }
        habit.UpdateFromDto(updateHabitDto);

        await dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpPatch("{id}")]
    public async Task<ActionResult> PatchHabit(string id, JsonPatchDocument<HabitDto> patchDocument, CancellationToken cancellationToken)
    {
        Habit? habit = await dbContext.Habits.FirstOrDefaultAsync(h => h.Id == id, cancellationToken);

        if (habit == null)
        {
            return NotFound();
        }

        HabitDto habitDto = habit.ToDto();
        patchDocument.ApplyTo(habitDto, ModelState);

        if (!TryValidateModel(habitDto))
        {
            return ValidationProblem(ModelState);
        }

        habit.Name = habitDto.Name;
        habit.Description = habitDto.Description;
        habit.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteHabit(string id, CancellationToken cancellationToken)
    {
        Habit? habit = await dbContext.Habits.FirstOrDefaultAsync(h => h.Id == id, cancellationToken);

        if (habit == null)
        {
            //return StatusCode(StatusCodes.Status410Gone); for soft delete
            return NotFound();
        }

        dbContext.Habits.Remove(habit);
        await dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}
