using System.Linq;
using APITutorial.API.Database;
using APITutorial.API.DTOs.Habits;
using APITutorial.API.DTOs.HabitTags;
using APITutorial.API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APITutorial.API.Controllers;

[ApiController]
[Route("api/habits/{habitId}/tags")]
public sealed class HabitTagsController(ApplicationDbContext dbContext) : ControllerBase
{
    public static readonly string Name = nameof(HabitTagsController).Replace("Controller", string.Empty, StringComparison.Ordinal);

    [HttpPut]
    public async Task<ActionResult> UpsertHabitTag(string habitId, UpsertHabitTagDto upsertHabitTagDto, CancellationToken cancellationToken)
    {
        var habit = await dbContext.Habits.Include(h => h.HabitTags).FirstOrDefaultAsync(h => h.Id == habitId, cancellationToken: cancellationToken);

        if (habit == null)
        {
            return NotFound();
        }

        var currentTagIds = habit.HabitTags.Select(ht => ht.TagId).ToHashSet();

        if (currentTagIds.SetEquals(upsertHabitTagDto.TagIds))
        {
            return NoContent();
        }

        List<string> existingTagIds = await dbContext.Tags.Where(t => upsertHabitTagDto.TagIds.Contains(t.Id)).Select(t => t.Id).ToListAsync(cancellationToken);

        if (existingTagIds.Count != upsertHabitTagDto.TagIds.Count)
        {
            return BadRequest("One or more TagIds is invalid.");
        }

        habit.HabitTags.RemoveAll(ht => !upsertHabitTagDto.TagIds.Contains(ht.TagId));

        string[] tagIdsToAdd = upsertHabitTagDto.TagIds.Except(currentTagIds).ToArray();
        habit.HabitTags.AddRange(tagIdsToAdd.Select(tagId => new Entities.HabitTag
        {
            HabitId = habitId,
            TagId = tagId,
            CreatedAtUtc=DateTime.UtcNow
        }));

        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok();
    }

    [HttpDelete]
    public async Task<ActionResult> DeleteHabitTag(string habitId, UpdateHabitDto updateHabitDto, CancellationToken cancellationToken)
    {
        HabitTag? habitTag = await dbContext.HabitTags.SingleOrDefaultAsync(ht => ht.HabitId == habitId, cancellationToken);

        if (habitTag == null)
        {
            return NotFound();
        }

        dbContext.HabitTags.Remove(habitTag);

        await dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

}
