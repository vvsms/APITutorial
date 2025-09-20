using System.Linq.Expressions;
using APITutorial.API.Entities;

namespace APITutorial.API.DTOs.Tags;

internal static class TagQueries
{
    public static Expression<Func<Tag, TagDto>> ProjectToDto()
    {
        return h => new TagDto
        {
            Id = h.Id,
            Name = h.Name,
            Description = h.Description,
            CreatedAtUtc = h.CreatedAtUtc,
            UpdatedAtUtc = h.UpdatedAtUtc,
        };
    }
}
