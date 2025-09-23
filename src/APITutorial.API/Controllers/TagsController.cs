using APITutorial.API.Database;
using APITutorial.API.DTOs.Tags;
using APITutorial.API.Entities;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Validations;

namespace APITutorial.API.Controllers;


[ApiController]
[Route("api/[controller]")]
public sealed class TagsController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<TagsCollectionDto>> GetTags(CancellationToken cancellationToken)
    {
        List<TagDto> tags = await dbContext.Tags.Select(TagQueries.ProjectToDto())
            .ToListAsync(cancellationToken);

        var TagsCollection = new TagsCollectionDto
        {
            Items = tags
        };
        return Ok(TagsCollection);
    }


    [HttpGet("{id}")]
    public async Task<ActionResult<TagDto>> GetTags(string id, CancellationToken cancellationToken)
    {
        TagDto? tags = await dbContext.Tags
            .Where(h => h.Id == id)
            .Select(TagQueries.ProjectToDto())
            .FirstOrDefaultAsync(cancellationToken);

        return tags == null ? (ActionResult<TagDto>)NotFound() : (ActionResult<TagDto>)Ok(tags);
    }

    [HttpPost]
    public async Task<ActionResult<TagDto>> CreateTag(CreateTagDto createTagDto, IValidator<CreateTagDto> validator, ProblemDetailsFactory problemDetailsFactory, CancellationToken cancellationToken)
    {

        ValidationResult validationResult = await validator.ValidateAsync(createTagDto, cancellationToken);

        if (!validationResult.IsValid)
        {
            ProblemDetails problem = problemDetailsFactory.CreateProblemDetails(HttpContext, StatusCodes.Status400BadRequest);

            problem.Extensions.Add("errors", validationResult.ToDictionary());

            return BadRequest(problem);
        }


        var tag = createTagDto.ToEntity();

        if (await dbContext.Tags.AnyAsync(h => h.Name == tag.Name, cancellationToken))
        {
            return Problem(
                detail: $"Tag with name '{tag.Name}' already exists.",
                statusCode: StatusCodes.Status409Conflict
                );
        }

        dbContext.Tags.Add(tag);

        await dbContext.SaveChangesAsync(cancellationToken);

        var tagDto = tag.ToDto();

        return CreatedAtAction(nameof(GetTags), new { id = tag.Id }, tagDto);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TagDto>> UpdateTag(string id, UpdateTagDto updateTagDto, CancellationToken cancellationToken)
    {
        Tag? tag = await dbContext.Tags.FirstOrDefaultAsync(h => h.Id == id, cancellationToken);

        if (tag == null)
        {
            return NotFound();
        }
        tag.UpdateFromDto(updateTagDto);

        await dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
    [HttpPatch("{id}")]
    public async Task<ActionResult> PatchTag(string id, JsonPatchDocument<TagDto> patchDocument, CancellationToken cancellationToken)
    {
        Tag? tag = await dbContext.Tags.FirstOrDefaultAsync(h => h.Id == id, cancellationToken);

        if (tag == null)
        {
            return NotFound();
        }

        TagDto tagDto = tag.ToDto();
        patchDocument.ApplyTo(tagDto, ModelState);

        if (!TryValidateModel(tagDto))
        {
            return ValidationProblem(ModelState);
        }

        tag.Name = tagDto.Name;
        tag.Description = tagDto.Description;
        tag.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteTag(string id, CancellationToken cancellationToken)
    {
        Tag? tag = await dbContext.Tags.FirstOrDefaultAsync(h => h.Id == id, cancellationToken);

        if (tag == null)
        {
            //return StatusCode(StatusCodes.Status410Gone); for soft delete
            return NotFound();
        }

        dbContext.Tags.Remove(tag);
        await dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}
