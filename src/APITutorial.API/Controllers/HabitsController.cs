using APITutorial.API.Database;
using APITutorial.API.DTOs.Habits;
using APITutorial.API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APITutorial.API.Controllers;


[ApiController]
[Route("api/[controller]")]
public sealed class HabitsController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<HabitsCollectionDto>> GetHabits(CancellationToken cancellationToken)
    {
        List<HabitDto> habits = await dbContext.Habits.Select(HabitQueries.ProjectToDto())
            .ToListAsync(cancellationToken);

        var habitsCollection = new HabitsCollectionDto
        {
            Data = habits
        };
        return Ok(habitsCollection);
    }


    [HttpGet("{id}")]
    public async Task<ActionResult<HabitDto>> GetHabits(string id, CancellationToken cancellationToken)
    {
        HabitDto? habits = await dbContext.Habits
            .Where(h => h.Id == id)
            .Select(HabitQueries.ProjectToDto())
            .FirstOrDefaultAsync(cancellationToken);

        return habits == null ? (ActionResult<HabitDto>)NotFound() : (ActionResult<HabitDto>)Ok(habits);
    }



    [HttpPost]
    public async Task<ActionResult<HabitDto>> CreateHabit(CreateHabitDto createHabitDto, CancellationToken cancellationToken)
    {
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


}
